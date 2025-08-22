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

        public void Initialize(BlockDatabase blockDatabase, BlockAtlasDatabase blockAtlasDatabase)
        {
            Mesh = new ChunkMeshBuilder(gameObject, blockDatabase, blockAtlasDatabase);
            Collider = new ChunkColliderBuilder(_polygonCollider2D, blockDatabase, blockAtlasDatabase);
        }

        public void Render(Chunk chunk)
        {
            Mesh.BuildMesh(chunk).ApplyMesh();
            Collider.BuildCollider(chunk).ApplyCollider();
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

            Mesh.ApplyMesh();
            Collider.ApplyCollider();

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