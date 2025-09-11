using Cysharp.Threading.Tasks;
using UnityEngine;
using World.Chunks;

namespace World.Entities
{
    public class EntityChunksPreloadWaiting : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private ChunksPreloadAt _chunksPreloadAt;
        private ChunksPreloader _worldChunksPreloader;

        public EntityChunksPreloadWaiting SetPreloader(ChunksPreloader worldChunksPreloader)
        {
            _worldChunksPreloader = worldChunksPreloader;
            return this;
        }

        public async UniTask StartWait()
        {
            _rigidbody.simulated = false;

            await _chunksPreloadAt.PreloadAsync(_worldChunksPreloader);
            
            _rigidbody.simulated = true;
        }
    }
}