using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using World.Chunks.BlocksStorage;
using World.Chunks.Generator;

namespace World.Chunks
{
    #region Интерфейс чтения
    public interface IWorldStorageAccessor
    {
        public bool TryGetChunk(ChunkIndex index, out Chunk chunk);
        public bool TryGetRenderer(ChunkIndex index, out ChunkRenderer renderer);
        public bool TryGetChunk(WorldPosition position, out Chunk chunk);
        public bool TryGetRenderer(WorldPosition position, out ChunkRenderer renderer);

        public void Dispose(ChunkIndex index);
        public void DisposeAll();

        public IEnumerable<ChunkIndex> VisibleCoords { get; }
        public IEnumerable<ChunkIndex> OutCoords { get; }
        public IEnumerable<ChunkIndex> AllCoords { get; }

        public IEnumerable<Chunk> VisibleChunks { get; }
        public IEnumerable<Chunk> OutChunks { get; }
        public IEnumerable<Chunk> AllChunks { get; }

        public IEnumerable<ChunkRenderer> VisibleRenderers { get; }
        public IEnumerable<ChunkRenderer> OutRenderers { get; }
        public IEnumerable<ChunkRenderer> AllRenderers { get; }
        
        public void RerenderAll();
    }
    #endregion

    public class WorldStorage : MonoBehaviour, IWorldStorageAccessor
    {
        #region Поля
        [SerializeField] private WorldChunksVisible _visibility;
        [SerializeField] private WorldChunksPreloader _preloader;
        [SerializeField] private WorldManager _manager;
        [SerializeField] private GameObject _prefab;
        [SerializeField] private Transform _chunksParent;

        public event Action OnVisibleChunksLoaded;
        public event Action OnVisibleChunksUpdated;

        public event Action OnOutChunksUpdated;

        [NonSerialized] public bool Loaded = false;

        private readonly Dictionary<ChunkIndex, Chunk> _chunks = new();
        private readonly Dictionary<ChunkIndex, Chunk> _outChunks = new();
        private readonly Dictionary<ChunkIndex, ChunkRenderer> _renderers = new();
        private readonly Dictionary<ChunkIndex, ChunkRenderer> _outRenderers = new();
        private readonly HashSet<ChunkIndex> _loadings = new();
        private IChunkGenerator _generator;

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

        #region Слушатели
        private void HandleVisibleChanged(RectInt rect)
        {
            _generator.CacheComputation(rect);
            _ = RefreshVisibleChunksAsync(rect);
        }
        public void HandleOutChanged(HashSet<ChunkIndex> newVisible)
        {
            _generator.CacheComputation(newVisible);
            _ = RefreshOutChunksAsync(newVisible);
        }
        #endregion

        #region Обновление чанков
        private async Task RefreshVisibleChunksAsync(RectInt rect)
        {
            Debug.Log($"RefreshVisibleChunksAsync started");

            HashSet<ChunkIndex> newVisible = new HashSet<ChunkIndex>();
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
        public async Task RefreshOutChunksAsync(HashSet<ChunkIndex> newVisible)
        {
            Debug.Log($"RefreshOutChunksAsync started");
            await RefreshChunksInIndexesAsync(newVisible, _outChunks, _outRenderers);

            OnOutChunksUpdated?.Invoke();
        }
        public async Task RefreshChunksInIndexesAsync(
            HashSet<ChunkIndex> newIndexes,
            Dictionary<ChunkIndex, Chunk> writeChunk,
            Dictionary<ChunkIndex, ChunkRenderer> writeRenderer
        )
        {
            var alreadyCreatedIndexes = writeChunk.Keys;
            var toRemove = alreadyCreatedIndexes.Except(newIndexes).ToList();
            DisposeAll(toRemove);

            var toCreate = newIndexes.Except(alreadyCreatedIndexes).ToList();
            await Task.WhenAll(toCreate.Select(async index =>
            {
                try
                {
                    await CreateChunkAsync(index, writeChunk, writeRenderer);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }));
        }
        #endregion

        #region Создание чанка
        public async Task<bool> CreateChunkAsync(
            ChunkIndex index,
            Dictionary<ChunkIndex, Chunk> writeChunk,
            Dictionary<ChunkIndex, ChunkRenderer> writeRenderer
        )
        {
            if (!TryGetChunk(index, out Chunk chunk) && !_loadings.Contains(index))
            {
                // Добавляем индекс чанка в структуру загрузки
                // чтобы не пытаться загрузить данный чанк дважды
                // и сделать проверку на нужность продолжения рендера
                _loadings.Add(index);

                // Генерируем данные чанка
                chunk = await _generator.GenerateChunkAsync(index);

                // Пока выполнялся асинхронный GenerateChunkAsync
                // чанк с данным индексом уже мог удалиться
                // поэтому проверяем остался ли он в _loadings
                if (!_loadings.Contains(index)) return false;

                // Подписываемся на события чанка
                _manager.Events.SubscribeToChunkEvents(chunk);

                // Создаём рендерер чанка
                var go = UnityEngine.Object.Instantiate(_prefab, _chunksParent);
                var renderer = go.GetComponent<ChunkRenderer>();
                renderer.Initialize(_manager.BlockDatabase, _manager.BlockAtlasDatabase);
                go.name = index.ToString();
                go.transform.localPosition = new Vector3(index.x * chunk.Size, index.y * chunk.Size, 0);

                // Сохраняем сразу вместе (до асинхронного выполнения рендера чтобы в случае ненужности отменить рендер)
                writeRenderer[index] = renderer;
                writeChunk[index] = chunk;

                await renderer.RenderAsync(chunk);
            }

            return true;
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
                // Отписываемся от событий чанка
                _manager.Events.UnsubscribeFromChunkEvents(chunk);

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
            foreach (var index in AllCoords)
                Dispose(index);
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
