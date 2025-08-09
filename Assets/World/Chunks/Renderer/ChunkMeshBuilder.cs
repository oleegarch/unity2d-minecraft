using UnityEngine;
using System.Collections.Generic;
using World.Blocks.Atlases;
using World.Blocks;
using World.Chunks.BlocksStorage;

namespace World.Chunks
{
    public class ChunkMeshBuilder
    {
        private struct RenderInfo
        {
            public BlockAtlasCategory MeshCategory;
            public int QuadIndex;
        }

        private readonly GameObject _gameObject;
        private readonly BlockDatabase _blockDatabase;
        private readonly BlockAtlasDatabase _blockAtlasDatabase;
        private readonly List<GameObject> _categoryObjects = new();
        private readonly Dictionary<BlockAtlasCategory, ChunkMeshData> _meshDatas = new();
        private readonly Dictionary<BlockIndex, RenderInfo> _blockIndexes = new();

        private Chunk _chunk;

        public ChunkMeshBuilder(GameObject gameObject, BlockDatabase blockDatabase, BlockAtlasDatabase blockAtlasDatabase)
        {
            _gameObject = gameObject;
            _blockDatabase = blockDatabase;
            _blockAtlasDatabase = blockAtlasDatabase;
        }

        public ChunkMeshBuilder BuildMesh(Chunk chunk)
        {
            _chunk = chunk;

            Dispose();
            SubscribeToChunkChanges();

            for (byte x = 0; x < chunk.Size; x++)
                for (byte y = 0; y < chunk.Size; y++)
                {
                    BlockIndex index = new BlockIndex(x, y);
                    if (!chunk.Render.TryGetBlockRenderId(index, out ushort id, out bool behind)) continue;

                    BlockInfo info = _blockDatabase.Get(id);
                    BlockAtlasInfo atlas = _blockAtlasDatabase.Get(info.AtlasCategory);

                    if (!_meshDatas.TryGetValue(atlas.Category, out ChunkMeshData meshData))
                    {
                        meshData = new ChunkMeshData(atlas);
                        _meshDatas[atlas.Category] = meshData;
                    }

                    int quadIndex = meshData.AddQuadAt(index, id, behind);

                    _blockIndexes.Add(index, new RenderInfo { MeshCategory = atlas.Category, QuadIndex = quadIndex });
                }

            return this;
        }
        public void CreateMesh(ChunkMeshData meshData)
        {
            GameObject go = meshData.ApplyMesh();

            go.transform.SetParent(_gameObject.transform, false);
            _categoryObjects.Add(go);
        }
        public void ApplyMesh()
        {
            foreach (var meshData in _meshDatas.Values)
                CreateMesh(meshData);
        }

        public void DrawBlock(BlockIndex index)
        {
            if (!_chunk.Render.TryGetBlockRenderId(index, out ushort id, out bool behind)) return;

            ChunkMeshData meshData;

            if (_blockIndexes.TryGetValue(index, out RenderInfo renderInfo))
            {
                meshData = _RedrawBlock(index, renderInfo, id, behind);
            }
            else
            {
                meshData = _DrawBlock(index, id, behind);
            }

            meshData.RefreshMesh();
        }
        public void EraseBlock(BlockIndex index)
        {
            if (!_blockIndexes.TryGetValue(index, out RenderInfo renderInfo)) return;

            ChunkMeshData meshData;

            if (_chunk.Render.TryGetBlockRenderId(index, out ushort id, out bool behind))
            {
                meshData = _RedrawBlock(index, renderInfo, id, behind);
            }
            else
            {
                meshData = _EraseBlock(index, renderInfo);
            }

            meshData.RefreshMesh();
        }

        private ChunkMeshData _DrawBlock(BlockIndex index, ushort id, bool behind)
        {
            BlockInfo info = _blockDatabase.Get(id);
            BlockAtlasInfo atlas = _blockAtlasDatabase.Get(info.AtlasCategory);

            if (!_meshDatas.TryGetValue(atlas.Category, out ChunkMeshData meshData))
            {
                meshData = new ChunkMeshData(atlas);
                CreateMesh(meshData);

                _meshDatas[atlas.Category] = meshData;
            }

            int quadIndex = meshData.AddQuadAt(index, id, behind);
            _blockIndexes.Add(index, new RenderInfo { MeshCategory = atlas.Category, QuadIndex = quadIndex });

            return meshData;
        }
        private ChunkMeshData _RedrawBlock(BlockIndex index, RenderInfo renderInfo, ushort id, bool behind)
        {
            BlockInfo info = _blockDatabase.Get(id);
            BlockAtlasInfo atlas = _blockAtlasDatabase.Get(info.AtlasCategory);
            ChunkMeshData meshData = _meshDatas[renderInfo.MeshCategory];

            if (renderInfo.MeshCategory != atlas.Category)
            {
                meshData = _EraseBlock(index, renderInfo);
                meshData.RefreshMesh();
                meshData = _DrawBlock(index, id, behind);
            }
            else
            {
                meshData.UpdateQuadAt(renderInfo.QuadIndex, id, behind);
            }

            return meshData;
        }
        private ChunkMeshData _EraseBlock(BlockIndex index, RenderInfo renderInfo)
        {
            ChunkMeshData meshData = _meshDatas[renderInfo.MeshCategory];
            BlockIndex? moved = meshData.RemoveQuadAt(renderInfo.QuadIndex);

            if (moved.HasValue && _blockIndexes.TryGetValue(moved.Value, out var mv))
            {
                mv.QuadIndex = renderInfo.QuadIndex;
                _blockIndexes[moved.Value] = mv;
            }

            _blockIndexes.Remove(index);

            return meshData;
        }

        private void OnChunkBlockSet(BlockIndex index, Block block, BlockLayer layer)
        {
            DrawBlock(index);
        }
        private void OnChunkBlockBroken(BlockIndex index, Block block, BlockLayer layer)
        {
            EraseBlock(index);
        }
        private void SubscribeToChunkChanges()
        {
            _chunk.Blocks.Events.OnBlockSet += OnChunkBlockSet;
            _chunk.Blocks.Events.OnBlockBroken += OnChunkBlockBroken;
        }
        private void UnsubscribeFromChunkChanges()
        {
            _chunk.Blocks.Events.OnBlockSet -= OnChunkBlockSet;
            _chunk.Blocks.Events.OnBlockBroken -= OnChunkBlockBroken;
        }
        
        public void Dispose()
        {
            UnsubscribeFromChunkChanges();

            _categoryObjects.ForEach(Object.DestroyImmediate);
            _categoryObjects.Clear();
            _blockIndexes.Clear();
            _meshDatas.Clear();
        }
    }
}