using Cysharp.Threading.Tasks;
using UnityEngine;
using World.Chunks;

namespace World.Entities
{
    /// <summary>
    /// Класс позволяет после ставна установить прелоадер чанков и запустить их прелодинг по заранее указанным точкам.
    /// 
    /// Использовался для предзагрузки чанков под сущностью чтобы она уже начала существовать вне поля видимости пользователя.
    /// Сейчас не нужен, так как теперь сущности просто зависают пока они не войдут в область видимости чанков.
    /// </summary>
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