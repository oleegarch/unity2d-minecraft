using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using World.Blocks;
using World.Blocks.Atlases;
using World.Chunks.BlocksStorage;

namespace World.Chunks
{
    [RequireComponent(typeof(Transform))]
    public class ChunkRenderer : MonoBehaviour, IDisposable
    {
        [SerializeField] private PolygonCollider2D _polygonCollider2D;

        public ChunkMeshBuilder Mesh { get; private set; }
        public ChunkColliderBuilder Collider { get; private set; }

        private CancellationTokenSource _cts;
        private bool _initialized;

        public void Initialize(BlockDatabase blockDatabase, BlockAtlasDatabase blockAtlasDatabase)
        {
            Mesh = new ChunkMeshBuilder(gameObject, blockDatabase, blockAtlasDatabase);
            Collider = new ChunkColliderBuilder(_polygonCollider2D, blockDatabase, blockAtlasDatabase);
            _initialized = true;
        }

        private void LateUpdate()
        {
            if (_initialized == false) return;
            Mesh.Refresh();
            Collider.Refresh();
        }

        // Синхронный рендер (если нужен сразу и коротко)
        public void Render(Chunk chunk)
        {
            Mesh.BuildMesh(chunk).ApplyMesh().Refresh();
            Collider.BuildCollider(chunk).ApplyCollider().Refresh();
        }

        // Асинхронный рендер с UniTask
        public async UniTask<bool> RenderAsync(Chunk chunk)
        {
            // Отменяем предыдущую задачу рендера
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                // Выполняем тяжёлую сборку в thread-pool
                await UniTask.RunOnThreadPool(() =>
                {
                    Mesh.BuildMesh(chunk);
                    Collider.BuildCollider(chunk);
                }, cancellationToken: token);

                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException)
            {
                return false;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }

            // Применение мэш/коллайдера должно быть в main thread
            await UniTask.SwitchToMainThread(token);

            if (token.IsCancellationRequested) return false;

            Mesh.ApplyMesh().Refresh();
            Collider.ApplyCollider().Refresh();

            return true;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            Mesh?.Dispose();
            Collider?.Dispose();

            if (gameObject != null)
                Destroy(gameObject);
        }
    }
}