using Cysharp.Threading.Tasks;
using UnityEngine;
using World.Chunks.Generator;
using World.Chunks.Storage;
using World.Entities;

namespace World.Chunks
{
    public class WorldManager : MonoBehaviour
    {
        [SerializeField] private WorldEnvironmentAccessor _environment;
        [SerializeField] private ChunksStorageDispatcher _storage;
        [SerializeField] private ChunksCreator _creator;
        [SerializeField] private WorldEntities _entities;
        [SerializeField] private ChunksVisible _visibility;
        [SerializeField] private ChunksPreloader _chunksPreloader;

        public WorldEnvironmentAccessor EnvironmentAccessor => _environment;
        public WorldBlockEvents Events { get; private set; }
        public IWorldGenerator Generator { get; private set; }
        public IWorldBlockModifier Blocks { get; private set; }

        private void Awake()
        {
            _ = Startup();
        }
        private async UniTask Startup()
        {
            _environment.Initialize();
            _storage.Initialize();

            await _storage.Load();
            await UniTask.SwitchToMainThread();

            Generator = _environment.CurrentWorldGenerator;
            Events = new WorldBlockEvents(_creator);
            Blocks = new WorldBlockModifier(_creator, Generator);

            EnableAll();
        }
        public void EnableAll()
        {
            Generator.RegisterWorldSystems(this);
            _creator.Enable();
            _chunksPreloader.Enable();
            _entities.Enable();
            _visibility.Enable();
        }
        public void DisableAll()
        {
            Generator.UnregisterWorldSystems(this);
            _creator.Disable();
            _chunksPreloader.Disable();
            _entities.Disable();
            _visibility.Disable();
        }
        private void OnDestroy()
        {
            Events.Dispose();
            DisableAll();
        }
    }
}