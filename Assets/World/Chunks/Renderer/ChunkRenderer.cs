using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
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

        public void Render(Chunk chunk)
        {
            Mesh.BuildMesh(chunk).ApplyMesh().Refresh();
            Collider.BuildCollider(chunk).ApplyCollider().Refresh();
        }
        public async Task<bool> RenderAsync(Chunk chunk)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            var token = _cts.Token;

            try
            {
                await Task.Run(() =>
                {
                    Mesh.BuildMesh(chunk);
                    Collider.BuildCollider(chunk);
                }, token);

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

            Mesh.ApplyMesh().Refresh();
            Collider.ApplyCollider().Refresh();

            return true;
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            Mesh.Dispose();
            Collider.Dispose();
            Destroy(gameObject);
        }
    }
}