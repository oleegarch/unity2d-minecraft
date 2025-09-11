using UnityEngine;
using World.Blocks;
using World.Blocks.Atlases;
using World.Chunks.Generator;
using World.Items;
using World.Entities;

namespace World.Chunks
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] private GameObject _chunkRendererPrefab;
        [SerializeField] private ChunkGeneratorConfig _chunkGeneratorConfig;
        [SerializeField] private WorldChunksCreator _storage;
        [SerializeField] private WorldEntities _entities;
        [SerializeField] private WorldChunksVisible _visibility;
        [SerializeField] private WorldChunksPreloader _chunksPreloader;
        [SerializeField] private int _seed;

        public BlockDatabase BlockDatabase => _chunkGeneratorConfig.BlockDatabase;
        public BlockAtlasDatabase BlockAtlasDatabase => _chunkGeneratorConfig.BlockAtlasDatabase;
        public ItemDatabase ItemDatabase => _chunkGeneratorConfig.ItemDatabase;
        public ItemCategoryDatabase ItemCategoryDatabase => _chunkGeneratorConfig.ItemCategoryDatabase;
        public EntityDatabase EntityDatabase => _chunkGeneratorConfig.EntityDatabase;

        public IChunkGenerator Generator { get; private set; }
        public WorldBlockEvents Events { get; private set; }
        public IWorldBlockModifier Blocks { get; private set; }

        private void Awake()
        {
            Generator = _chunkGeneratorConfig.GetChunkGenerator(_seed);
            Events = new WorldBlockEvents();
            Blocks = new WorldBlockModifier(_storage, Generator);
        }
        private void Start()
        {
            Generator.RegisterWorldSystems(this);
        }
        private void OnEnable()
        {
            _storage.Enable();
            _chunksPreloader.Enable();
            _entities.Enable();
            _visibility.Enable();
        }
        private void OnDisable()
        {
            _storage.Disable();
            _chunksPreloader.Disable();
            _entities.Disable();
            _visibility.Disable();
        }
        private void OnDestroy()
        {
            Generator.UnregisterWorldSystems(this);
            _storage.DisposeAll();
        }
    }
}