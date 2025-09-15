using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using World.Chunks.Blocks;
using World.Chunks.Generator;
using Logger = World.Utils.Debugging.Logger;

namespace World.Chunks
{
    #region Интерфейс чтения
    public interface IChunksAccessor
    {
        public bool TryGetChunk(ChunkIndex index, out Chunk chunk);
        public bool TryGetRenderer(ChunkIndex index, out ChunkRenderer renderer);
        public bool TryGetChunk(WorldPosition position, out Chunk chunk);
        public bool TryGetRenderer(WorldPosition position, out ChunkRenderer renderer);

        public void Dispose(ChunkIndex index);
        public void DisposeAll();
        public void RerenderAll();
        
        public Subject<Chunk> OnChunkCreated { get; }
        public Subject<Chunk> OnChunkBeforeRemove { get; }

        public IEnumerable<ChunkIndex> AllCoords { get; }
        public IEnumerable<Chunk> AllChunks { get; }
        public IEnumerable<ChunkRenderer> AllRenderers { get; }
    }
    #endregion

    public class ChunksCreator : MonoBehaviour, IChunksAccessor
    {
        #region Поля и события
        [SerializeField] private ChunksVisible _visibility;
        [SerializeField] private ChunksPreloader _preloader;
        [SerializeField] private WorldManager _manager;
        [SerializeField] private WorldEnvironmentAccessor _environment;
        [SerializeField] private Transform _chunksParent;

        public event Action OnVisibleChunksLoaded;
        public event Action OnVisibleChunksUpdated;

        public event Action OnOutChunksUpdated;

        public Subject<Chunk> OnChunkCreated { get; private set; } = new();
        public Subject<Chunk> OnChunkBeforeRemove { get; private set; } = new();

        [NonSerialized] public bool Loaded = false;

        private readonly Dictionary<ChunkIndex, Chunk> _chunks = new();
        private readonly Dictionary<ChunkIndex, Chunk> _outChunks = new();
        private readonly Dictionary<ChunkIndex, ChunkRenderer> _renderers = new();
        private readonly Dictionary<ChunkIndex, ChunkRenderer> _outRenderers = new();
        private readonly HashSet<ChunkIndex> _loadings = new();
        private IWorldGenerator _generator;
        #endregion

        #region Жизненный цикл
        private void OnDestroy()
        {
            DisposeAll();
        }
        public void Enable()
        {
            _generator = _manager.Generator;
            _visibility.OnVisibleChunksChanged += HandleVisibleChanged;
            _preloader.OnChunksPreload += HandleOutChanged;
        }
        public void Disable()
        {
            _visibility.OnVisibleChunksChanged -= HandleVisibleChanged;
            _preloader.OnChunksPreload -= HandleOutChanged;
        }
        #endregion

        #region Удаление чанков
        public void Dispose(ChunkIndex index)
        {
            ChunkRenderer renderer;
            if (TryGetRenderer(index, out renderer))
            {
                renderer.Dispose();
                _renderers.Remove(index);
                _outRenderers.Remove(index);
            }

            Chunk chunk;
            if (TryGetChunk(index, out chunk))
            {
                OnChunkBeforeRemove.OnNext(chunk);
                chunk.Dispose();
                _chunks.Remove(index);
                _outChunks.Remove(index);
            }

            _loadings.Remove(index);
        }
        public void DisposeAll(List<ChunkIndex> indexes)
        {
            foreach (var index in indexes)
                Dispose(index);
        }
        public void DisposeAll()
        {
            foreach (var index in AllCoords.ToList())
                Dispose(index);
        }
        #endregion

        #region Слушатели
        private void HandleVisibleChanged(RectInt rect)
        {
            Debug.Log($"HandleVisibleChanged {rect}");
            _generator.CacheComputation(_visibility.BlocksVisibleRect);
            RefreshVisibleChunksAsync(rect).Forget();
        }
        public void HandleOutChanged(HashSet<ChunkIndex> newVisible)
        {
            _generator.CacheComputation(newVisible);
            RefreshOutChunksAsync(newVisible).Forget();
        }
        #endregion

        #region Обновление чанков
        private async UniTask RefreshVisibleChunksAsync(RectInt rect)
        {
            var newVisible = new HashSet<ChunkIndex>();
            for (int x = rect.xMin; x <= rect.xMax; x++)
                for (int y = rect.yMin; y <= rect.yMax; y++)
                    newVisible.Add(new ChunkIndex(x, y));

            await RefreshChunksInIndexesAsync(newVisible, _chunks, _renderers);

            if (Loaded == false)
            {
                Loaded = true;
                OnVisibleChunksLoaded?.Invoke();
            }

            OnVisibleChunksUpdated?.Invoke();
        }
        private async UniTask RefreshOutChunksAsync(HashSet<ChunkIndex> newVisible)
        {
            await RefreshChunksInIndexesAsync(newVisible, _outChunks, _outRenderers);

            OnOutChunksUpdated?.Invoke();
        }

        public async UniTask RefreshChunksInIndexesAsync(
            HashSet<ChunkIndex> newIndexes,
            Dictionary<ChunkIndex, Chunk> writeChunk,
            Dictionary<ChunkIndex, ChunkRenderer> writeRenderer
        )
        {
            // snapshot существующих
            var alreadyCreatedIndexes = writeChunk.Keys.ToList();
            var toRemove = alreadyCreatedIndexes.Except(newIndexes).ToList();
            var toCreate = newIndexes.Except(alreadyCreatedIndexes).ToList();

            // удаляем ненужные чанки
            DisposeAll(toRemove);

            // параллельно создаём чанки
            var tasks = toCreate.Select(index => CreateChunkAsync(index, writeChunk, writeRenderer)).ToArray();
            await UniTask.WhenAll(tasks);
        }
        #endregion

        #region Создание чанка
        private async UniTask<bool> CreateChunkAsync(
            ChunkIndex index,
            Dictionary<ChunkIndex, Chunk> writeChunk,
            Dictionary<ChunkIndex, ChunkRenderer> writeRenderer
        )
        {
            if (!TryGetChunk(index, out Chunk chunk) && !_loadings.Contains(index))
            {
                _loadings.Add(index);

                try
                {
                    // Генерация чанка
                    Logger.DevLog($"Chunk generate started {index}");
                    chunk = await _generator.GenerateChunkAsync(index);
                    Logger.DevLog($"Chunk generated {chunk.Index}");

                    // Инстантиируем префаб и инициализируем рендерер — это должно быть в main thread
                    await UniTask.SwitchToMainThread();

                    // Возможно chunk был удалён/отменён между вызовами — проверим
                    if (!_loadings.Contains(index))
                    {
                        Logger.DevLog("Chunk creating canceled after chunk was generated");
                        chunk.Dispose();
                        return false;
                    }

                    // Инстантиируем префаб
                    var go = UnityEngine.Object.Instantiate(_generator.ChunkPrefab, _chunksParent);
                    var renderer = go.GetComponent<ChunkRenderer>();
                    renderer.Initialize(_environment.BlockDatabase, _environment.BlockAtlasDatabase);
                    go.name = index.ToString();
                    go.transform.localPosition = new Vector3(index.x * chunk.Size, index.y * chunk.Size, 0);

                    // Сохраняем немедленно (чтобы другие части кода могли найти рендерер/чанк)
                    writeRenderer[index] = renderer;
                    writeChunk[index] = chunk;

                    // Уведомляем о создании чанка
                    OnChunkCreated.OnNext(chunk);

                    // Ждём асинхронный рендер
                    await renderer.RenderAsync(chunk);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    // при ошибке очищаем состояние
                    if (_loadings.Contains(index)) _loadings.Remove(index);
                    return false;
                }
                finally
                {
                    // убедимся, что индекс убран из _loadings, если он всё ещё там
                    _loadings.Remove(index);
                }
            }

            return true;
        }
        #endregion

        #region Операции GET
        public bool TryGetChunk(ChunkIndex index, out Chunk chunk)
            => _chunks.TryGetValue(index, out chunk) || _outChunks.TryGetValue(index, out chunk);

        public bool TryGetRenderer(ChunkIndex index, out ChunkRenderer renderer)
            => _renderers.TryGetValue(index, out renderer) || _outRenderers.TryGetValue(index, out renderer);

        public bool TryGetChunk(WorldPosition position, out Chunk chunk)
        {
            ChunkIndex chunkIndex = position.ToChunkIndex(_generator.ChunkSize);
            return TryGetChunk(chunkIndex, out chunk);
        }
        public bool TryGetRenderer(WorldPosition position, out ChunkRenderer renderer)
        {
            ChunkIndex chunkIndex = position.ToChunkIndex(_generator.ChunkSize);
            return TryGetRenderer(chunkIndex, out renderer);
        }

        public IEnumerable<ChunkIndex> VisibleCoords => _chunks.Keys;
        public IEnumerable<ChunkIndex> OutCoords => _outChunks.Keys;
        public IEnumerable<ChunkIndex> AllCoords => VisibleCoords.Concat(OutCoords);

        public IEnumerable<Chunk> VisibleChunks => _chunks.Values;
        public IEnumerable<Chunk> OutChunks => _outChunks.Values;
        public IEnumerable<Chunk> AllChunks => VisibleChunks.Concat(OutChunks);
        
        public IEnumerable<ChunkRenderer> VisibleRenderers => _renderers.Values;
        public IEnumerable<ChunkRenderer> OutRenderers => _outRenderers.Values;
        public IEnumerable<ChunkRenderer> AllRenderers => VisibleRenderers.Concat(OutRenderers);
        #endregion

        #region Утилиты
        public void RerenderAll()
        {
            DisposeAll();
            HandleVisibleChanged(_visibility.VisibleRect);
        }
        #endregion
    }
}