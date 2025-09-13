using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using World.Chunks.Blocks;
using World.Chunks.Generator;
using System.Threading;

namespace World.Chunks
{
    #region Интерфейс чтения
    public interface IWorldChunksAccessor
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

    public class WorldChunksCreator : MonoBehaviour, IWorldChunksAccessor
    {
        #region Поля
        [SerializeField] private ChunksVisible _visibility;
        [SerializeField] private ChunksPreloader _preloader;
        [SerializeField] private WorldManager _manager;
        [SerializeField] private WorldEnvironmentAccessor _environment;
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
        private IWorldGenerator _generator;

        // Cancellation tokens for current refresh operations
        private CancellationTokenSource _visibleCts;
        private CancellationTokenSource _outCts;

        #endregion

        #region Жизненный цикл
        private void OnDestroy()
        {
            CancelAllRefreshes();
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
            CancelAllRefreshes();
        }
        #endregion

        #region Слушатели
        private void HandleVisibleChanged(RectInt rect)
        {
            _visibleCts?.Cancel();
            _visibleCts = new CancellationTokenSource();

            _generator.CacheComputation(_visibility.BlocksVisibleRect);
            RefreshVisibleChunksAsync(rect, _visibleCts.Token).Forget();
        }

        public void HandleOutChanged(HashSet<ChunkIndex> newVisible)
        {
            _outCts?.Cancel();
            _outCts = new CancellationTokenSource();

            _generator.CacheComputation(newVisible);
            RefreshOutChunksAsync(newVisible, _outCts.Token).Forget();
        }
        #endregion

        #region Обновление чанков
        private async UniTask RefreshVisibleChunksAsync(RectInt rect, CancellationToken token = default)
        {
            var newVisible = new HashSet<ChunkIndex>();
            for (int x = rect.xMin; x <= rect.xMax; x++)
                for (int y = rect.yMin; y <= rect.yMax; y++)
                    newVisible.Add(new ChunkIndex(x, y));

            await RefreshChunksInIndexesAsync(newVisible, _chunks, _renderers, token);

            if (token.IsCancellationRequested) return;

            if (Loaded == false)
            {
                Loaded = true;
                OnVisibleChunksLoaded?.Invoke();
            }

            OnVisibleChunksUpdated?.Invoke();
        }

        public async UniTask RefreshOutChunksAsync(HashSet<ChunkIndex> newVisible, CancellationToken token = default)
        {
            await RefreshChunksInIndexesAsync(newVisible, _outChunks, _outRenderers, token);

            if (token.IsCancellationRequested) return;

            OnOutChunksUpdated?.Invoke();
        }

        public async UniTask RefreshChunksInIndexesAsync(
            HashSet<ChunkIndex> newIndexes,
            Dictionary<ChunkIndex, Chunk> writeChunk,
            Dictionary<ChunkIndex, ChunkRenderer> writeRenderer,
            CancellationToken token = default
        )
        {
            // snapshot существующих
            var alreadyCreatedIndexes = writeChunk.Keys.ToList();
            var toRemove = alreadyCreatedIndexes.Except(newIndexes).ToList();
            var toCreate = newIndexes.Except(alreadyCreatedIndexes).ToList();

            // удаляем ненужные чанки
            DisposeAll(toRemove);

            // параллельно создаём чанки
            var tasks = toCreate.Select(index => CreateChunkAsync(index, writeChunk, writeRenderer, token)).ToArray();
            await UniTask.WhenAll(tasks);
        }
        #endregion

        #region Создание чанка
        public async UniTask<bool> CreateChunkAsync(
            ChunkIndex index,
            Dictionary<ChunkIndex, Chunk> writeChunk,
            Dictionary<ChunkIndex, ChunkRenderer> writeRenderer,
            CancellationToken token = default
        )
        {
            if (token.IsCancellationRequested) return false;

            if (!TryGetChunk(index, out Chunk chunk) && !_loadings.Contains(index))
            {
                _loadings.Add(index);

                try
                {
                    // Генерация чанка
                    Chunk generatedChunk = await _generator.GenerateChunkAsync(index);

                    if (token.IsCancellationRequested)
                    {
                        // отменено после генерации — чистим и выходим
                        _loadings.Remove(index);
                        return false;
                    }

                    chunk = generatedChunk;

                    // Подписываемся на события чанка
                    _manager.Events.SubscribeToChunkEvents(chunk);

                    // Инстантиируем префаб и инициализируем рендерер — это должно быть в main thread
                    await UniTask.SwitchToMainThread(token);

                    // Возможно chunk был удалён/отменён между вызовами — проверим
                    if (!_loadings.Contains(index) || token.IsCancellationRequested)
                    {
                        // Если отменили, удаляем чанк, отписываем, и выходим
                        _manager.Events.UnsubscribeFromChunkEvents(chunk);
                        chunk.Dispose();
                        _loadings.Remove(index);
                        return false;
                    }

                    var go = UnityEngine.Object.Instantiate(_prefab, _chunksParent);
                    var renderer = go.GetComponent<ChunkRenderer>();
                    renderer.Initialize(_environment.BlockDatabase, _environment.BlockAtlasDatabase);
                    go.name = index.ToString();
                    go.transform.localPosition = new Vector3(index.x * chunk.Size, index.y * chunk.Size, 0);

                    // Сохраняем немедленно (чтобы другие части кода могли найти рендерер/чанк)
                    writeRenderer[index] = renderer;
                    writeChunk[index] = chunk;

                    // Ждём асинхронный рендер
                    await renderer.RenderAsync(chunk).AttachExternalCancellation(token);
                }
                catch (OperationCanceledException)
                {
                    // просто возвращаем false при отмене
                    return false;
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
            foreach (var index in AllCoords.ToList())
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

        private void CancelAllRefreshes()
        {
            _visibleCts?.Cancel();
            _visibleCts?.Dispose();
            _visibleCts = null;

            _outCts?.Cancel();
            _outCts?.Dispose();
            _outCts = null;
        }
        #endregion
    }
}