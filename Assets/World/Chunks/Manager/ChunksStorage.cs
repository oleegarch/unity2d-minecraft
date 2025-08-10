using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using World.Chunks.BlocksStorage;
using World.Chunks.Generator;

namespace World.Chunks
{
    public interface IChunksStorage
    {
        public Task<bool> GetOrCreateAsync(ChunkIndex index, int seed);

        public bool TryGetChunk(ChunkIndex index, out Chunk chunk);
        public bool TryGetRenderer(ChunkIndex index, out ChunkRenderer renderer);
        public bool TryGetChunk(WorldPosition position, out Chunk chunk);
        public bool TryGetRenderer(WorldPosition position, out ChunkRenderer renderer);

        public void Dispose(ChunkIndex index);
        public void DisposeAll();

        public IEnumerable<ChunkIndex> GetAllCoords();
        public IEnumerable<ChunkIndex> GetAllVisibleCoords();
        public IEnumerable<Chunk> GetAllChunks();
        public IEnumerable<ChunkRenderer> GetAllRenderers();
    }

    public class ChunksStorage : IChunksStorage
    {
        private readonly IChunkGenerator _generator;
        private readonly ChunksManager _manager;
        private readonly GameObject _prefab;
        private readonly Transform _parent;
        private readonly Dictionary<ChunkIndex, Chunk> _chunks = new();
        private readonly Dictionary<ChunkIndex, ChunkRenderer> _renderers = new();
        private readonly HashSet<ChunkIndex> _loadings = new();

        public ChunksStorage(IChunkGenerator generator, GameObject prefab, Transform parent, ChunksManager manager)
        {
            _generator = generator;
            _prefab = prefab;
            _parent = parent;
            _manager = manager;
        }

        public async Task<bool> GetOrCreateAsync(ChunkIndex index, int seed)
        {
            if (!_chunks.TryGetValue(index, out var chunk) && !_loadings.Contains(index))
            {
                // Добавляем индекс чанка в структуру загрузки
                // чтобы не пытаться загрузить данный чанк дважды
                // и сделать проверку на нужность продолжения рендера
                _loadings.Add(index);

                // Генерируем данные чанка
                chunk = await _generator.GenerateChunkAsync(index, seed);

                // Создаём рендерер чанка
                var go = Object.Instantiate(_prefab, _parent);
                var renderer = go.GetComponent<ChunkRenderer>();
                renderer.Initialize(_manager.BlockDatabase, _manager.BlockAtlasDatabase);
                go.name = index.ToString();
                go.transform.localPosition = new Vector3(index.x * chunk.Size, index.y * chunk.Size, 0);

                // Сохраняем сразу вместе (до асинхронного выполнения рендера чтобы в случае ненужности отменить рендер)
                _renderers[index] = renderer;
                _chunks[index] = chunk;

                // Пока выполнялся асинхронный GenerateChunkAsync
                // чанк с данным индексом уже мог удалиться
                // поэтому проверяем остался ли он в _loadings
                if (!_loadings.Contains(index))
                {
                    Dispose(index);
                    return false;
                }

                await renderer.RenderAsync(chunk);
            }

            return true;
        }

        public void Dispose(ChunkIndex index)
        {
            if (_renderers.TryGetValue(index, out var renderer))
            {
                renderer.Dispose();
                _renderers.Remove(index);
            }
            if (_chunks.TryGetValue(index, out var chunk))
            {
                chunk.Dispose();
                _chunks.Remove(index);
            }
            _loadings.Remove(index);
        }
        public void DisposeAll()
        {
            foreach (var index in _chunks.Keys.ToList())
            {
                Dispose(index);
            }
        }

        public bool TryGetChunk(ChunkIndex index, out Chunk chunk)
            => _chunks.TryGetValue(index, out chunk);

        public bool TryGetRenderer(ChunkIndex index, out ChunkRenderer renderer)
            => _renderers.TryGetValue(index, out renderer);

        public bool TryGetChunk(WorldPosition position, out Chunk chunk)
        {
            ChunkIndex chunkIndex = position.ToChunkIndex(_generator.ChunkSize);
            return _chunks.TryGetValue(chunkIndex, out chunk);
        }
        public bool TryGetRenderer(WorldPosition position, out ChunkRenderer renderer)
        {
            ChunkIndex chunkIndex = position.ToChunkIndex(_generator.ChunkSize);
            return _renderers.TryGetValue(chunkIndex, out renderer);
        }

        public IEnumerable<ChunkIndex> GetAllCoords() => _chunks.Keys;
        public IEnumerable<ChunkIndex> GetAllVisibleCoords() => _renderers.Keys;
        public IEnumerable<Chunk> GetAllChunks() => _chunks.Values;
        public IEnumerable<ChunkRenderer> GetAllRenderers() => _renderers.Values;
    }
}
