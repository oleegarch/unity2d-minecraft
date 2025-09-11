using UnityEngine;
using World.Chunks;

namespace World.Entities
{
    public class EntityChunksDynamicPreloading : MonoBehaviour
    {
        [SerializeField] private float _watchMoveCallInterval = 1f;
        [SerializeField] private Transform _watchMoveTransform;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private ChunksPreloadAt _chunksPreloadAt;
        private ChunksPreloader _worldChunksPreloader;
        private Vector2 _preloadedAtPosition;

        public EntityChunksDynamicPreloading SetPreloader(ChunksPreloader worldChunksPreloader)
        {
            _worldChunksPreloader = worldChunksPreloader;
            return this;
        }

        private void Awake()
        {
            Preload();
        }
        private void Update()
        {
            if (Vector2.Distance(_preloadedAtPosition, (Vector2)_watchMoveTransform.position) > _watchMoveCallInterval)
                Preload();
        }

        private void Preload()
        {
            if (_worldChunksPreloader == null) return;
            _preloadedAtPosition = _watchMoveTransform.position;
            _chunksPreloadAt.Preload(_worldChunksPreloader);
        }
    }
}