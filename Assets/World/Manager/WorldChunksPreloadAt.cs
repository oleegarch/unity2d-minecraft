using Cysharp.Threading.Tasks;
using UnityEngine;

namespace World.Chunks
{
    public class WorldChunksPreloadAt : MonoBehaviour
    {
        [SerializeField] private Transform[] _points;

        private WorldPosition[] _cachedPositions;
        private WorldChunksPreloader _preloader;

        private void Awake()
        {
            _cachedPositions = new WorldPosition[_points.Length];
        }
        private void OnDestroy()
        {
            DestroyPreloadComponent();
        }

        public void Preload(WorldChunksPreloader preloader)
        {
            DestroyPreloadComponentIfNeed(preloader);
            _preloader = preloader;
            _preloader.SetComponentPositions(gameObject.GetInstanceID(), GetWorldPositions());
        }
        public async UniTask PreloadAsync(WorldChunksPreloader preloader)
        {
            var ucs = new UniTaskCompletionSource();

            void Handler()
            {
                preloader.OnChunksPreloaded -= Handler;
                ucs.TrySetResult();
            }

            preloader.OnChunksPreloaded += Handler;

            Preload(preloader);

            await ucs.Task;
        }

        public void DestroyPreloadComponent()
        {
            _preloader?.DestroyComponent(gameObject.GetInstanceID());
        }
        public void DestroyPreloadComponentIfNeed(WorldChunksPreloader newPreloader)
        {
            if (_preloader != null && newPreloader != _preloader)
            {
                DestroyPreloadComponent();
            }
        }

        public WorldPosition[] GetWorldPositions()
        {
            for (int i = 0; i < _points.Length; i++)
            {
                Vector3 position = _points[i].position;
                _cachedPositions[i] = new WorldPosition((int)position.x, (int)position.y);
            }

            return _cachedPositions;
        }
    }
}