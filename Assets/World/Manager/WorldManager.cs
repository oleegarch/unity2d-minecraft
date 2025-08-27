using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using World.Blocks;
using World.Blocks.Atlases;
using World.Chunks.Generator;
using World.Items;

namespace World.Chunks
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] private GameObject _chunkRendererPrefab;
        [SerializeField] private Transform _chunksParent;
        [SerializeField] private ChunkGeneratorConfig _chunkGeneratorConfig;
        [SerializeField] private WorldVisibleService _visibility;
        [SerializeField] private int _seed;

        private int _version;

        public event Action OnVisibleChunksLoaded;
        public event Action OnVisibleChunksUpdated;
        public event Action OnVisibleChunksDestroyed;

        public BlockDatabase BlockDatabase => _chunkGeneratorConfig.BlockDatabase;
        public BlockAtlasDatabase BlockAtlasDatabase => _chunkGeneratorConfig.BlockAtlasDatabase;
        public ItemDatabase ItemDatabase => _chunkGeneratorConfig.ItemDatabase;

        public IChunkGenerator Generator { get; private set; }
        public IWorldBlockModifier Blocks { get; private set; }
        public IWorldStorage Storage { get; private set; }

        [NonSerialized] public bool Loaded = false;

        private void Awake()
        {
            Generator = _chunkGeneratorConfig.GetChunkGenerator(_seed);
            Storage = new WorldStorage(Generator, _chunkRendererPrefab, _chunksParent, this);
            Blocks = new WorldBlockModifier(Storage, Generator.Rules);
        }
        private void Start()
        {
            Generator.RegisterWorldSystems(this);
        }

        private void OnEnable()
        {
            _visibility.OnVisibleChunksChanged += HandleVisibleChanged;
        }
        private void OnDisable()
        {
            _visibility.OnVisibleChunksChanged -= HandleVisibleChanged;
        }
        private void OnDestroy()
        {
            Generator.UnregisterWorldSystems(this);
            Storage.DisposeAll();
            OnVisibleChunksDestroyed?.Invoke();
        }

        private void HandleVisibleChanged(RectInt viewRect)
        {
            Generator.CacheComputation(viewRect);
            
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

                    try
                    {
                        await Storage.GetOrCreateAsync(index);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

            var toRemove = new List<ChunkIndex>();
            foreach (var kvp in Storage.GetAllCoords().ToList())
                if (!needed.Contains(kvp))
                    toRemove.Add(kvp);

            foreach (ChunkIndex index in toRemove)
                Storage.Dispose(index);

            if (Loaded == false)
            {
                Loaded = true;
                OnVisibleChunksLoaded?.Invoke();
            }

            OnVisibleChunksUpdated?.Invoke();
        }

        public void RerenderAll()
        {
            Storage.DisposeAll();

            HandleVisibleChanged(_visibility.VisibleRect);
        }
    }
}