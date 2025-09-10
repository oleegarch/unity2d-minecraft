using System.Threading.Tasks;
using UnityEngine;
using World.Chunks;

namespace World.Entities
{
    public class EntityChunksPreloadWaiting : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private WorldChunksPreloadAt _chunksPreloadAt;
        private WorldChunksPreloader _worldChunksPreloader;

        public EntityChunksPreloadWaiting SetPreloader(WorldChunksPreloader worldChunksPreloader)
        {
            _worldChunksPreloader = worldChunksPreloader;
            return this;
        }

        public async Task StartWait()
        {
            _rigidbody.simulated = false;

            await _chunksPreloadAt.PreloadAsync(_worldChunksPreloader);
            
            _rigidbody.simulated = true;
        }
    }
}