using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using World.BlockHovered;
using World.Chunks.Generator;
using System.Linq;

namespace World.Chunks
{
    public class ChunksManager : MonoBehaviour
    {
        [SerializeField] private GameObject _chunkRendererPrefab;
        [SerializeField] private Transform _chunksParent;
        [SerializeField] private ChunkGeneratorConfig _chunkGeneratorConfig;
        [SerializeField] private BlockBreakingProcess _breaker;
        [SerializeField] private ChunksVisibleService _visibility;
        [SerializeField] private int _seed;

        private int _version;

        public event Action OnVisibleChunksUpdated;
        public IChunksBlockModifier Block { get; private set; }
        public IChunksStorage Storage { get; private set; }

        [NonSerialized] public bool Loaded = false;

        private void Awake()
        {
            var generator = _chunkGeneratorConfig.GetChunkGenerator();
            Storage = new ChunksStorage(generator, _chunkRendererPrefab, _chunksParent);
            Block = new ChunksBlockModifier(Storage);
        }

        private void OnEnable()
        {
            _visibility.OnVisibleChunksChanged += HandleVisibleChanged;
            _breaker.OnBlockBroken += HandleBlockBroken;
        }

        private void OnDisable()
        {
            _visibility.OnVisibleChunksChanged -= HandleVisibleChanged;
            _breaker.OnBlockBroken -= HandleBlockBroken;
        }

        private void HandleVisibleChanged(RectInt viewRect)
        {
            int version = ++_version;
            _ = UpdateVisibleAsync(viewRect, version);
        }

        private async Task UpdateVisibleAsync(RectInt rect, int version)
        {
            var needed = new HashSet<ChunkIndex>();

            for (int x = rect.xMin; x <= rect.xMax; x++)
                for (int y = rect.yMin; y <= rect.yMax; y++)
                {
                    if (version != _version) return;

                    ChunkIndex index = new ChunkIndex(x, y);
                    needed.Add(index);

                    await Storage.GetOrCreateAsync(index, _seed);
                }

            var toRemove = new List<ChunkIndex>();
            foreach (var kvp in Storage.GetAllCoords().ToList())
                if (!needed.Contains(kvp))
                    toRemove.Add(kvp);

            foreach (ChunkIndex index in toRemove)
                Storage.Dispose(index);

            Loaded = true;
            OnVisibleChunksUpdated?.Invoke();
        }

        public void RerenderAll()
        {
            Storage.DisposeAll();

            HandleVisibleChanged(_visibility.VisibleRect);
        }

        private void HandleBlockBroken(WorldPosition wc) => Block.BreakVisible(wc);
    }
}