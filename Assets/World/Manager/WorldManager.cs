using UnityEngine;
using World.Chunks.Generator;
using World.Entities;

namespace World.Chunks
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] private GameObject _chunkRendererPrefab;
        [SerializeField] private WorldEnvironmentAccessor _environment;
        [SerializeField] private WorldChunksCreator _storage;
        [SerializeField] private WorldEntities _entities;
        [SerializeField] private ChunksVisible _visibility;
        [SerializeField] private ChunksPreloader _chunksPreloader;

        public WorldEnvironmentAccessor EnvironmentAccessor => _environment;
        public WorldBlockEvents Events { get; private set; }
        public IWorldGenerator Generator { get; private set; }
        public IWorldBlockModifier Blocks { get; private set; }

        private void Awake()
        {
            _environment.Initialize();
            
            Generator = _environment.CurrentWorldGenerator;
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
        }
    }
}