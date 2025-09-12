using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using World.Blocks.Atlases;
using World.Blocks;
using World.Chunks.Blocks;

namespace World.Chunks
{
    public class ChunkMeshBuilder
    {
        private struct RenderInfo
        {
            public BlockAtlasCategory MeshCategory;
            public int QuadIndex;
            public ushort BlockId;
            public bool Behind;
        }

        private readonly Transform _categoriesParent;
        private readonly BlockDatabase _blockDatabase;
        private readonly BlockAtlasDatabase _blockAtlasDatabase;
        private readonly List<GameObject> _categoryObjects = new();
        private readonly Dictionary<BlockAtlasCategory, ChunkMeshData> _meshDatas = new();
        private readonly Dictionary<BlockIndex, List<RenderInfo>> _blockIndexes = new();
        private readonly ChunkMeshesRefresher _refresher = new();

        private Chunk _chunk;

        public ChunkMeshBuilder(GameObject gameObject, BlockDatabase blockDatabase, BlockAtlasDatabase blockAtlasDatabase)
        {
            _categoriesParent = gameObject.transform;
            _blockDatabase = blockDatabase;
            _blockAtlasDatabase = blockAtlasDatabase;
        }

        public void Refresh() => _refresher.Refresh();

        public ChunkMeshBuilder BuildMesh(Chunk chunk)
        {
            UnsubscribeFromChunkChanges();

            _chunk = chunk;

            ClearAll();
            SubscribeToChunkChanges();

            for (byte x = 0; x < chunk.Size; x++)
                for (byte y = 0; y < chunk.Size; y++)
                {
                    BlockIndex index = new BlockIndex(x, y);

                    // Получаем стэк слоёв (back..front)
                    var layers = _chunk.Render.GetRenderStack(index, _blockDatabase, _blockAtlasDatabase);
                    if (layers == null || layers.Count == 0) continue;

                    // Для каждого слоя добавляем quad
                    foreach (var layer in layers)
                    {
                        BlockInfo info = _blockDatabase.Get(layer.Id);
                        BlockAtlasInfo atlas = _blockAtlasDatabase.Get(info.AtlasCategory);

                        if (!_meshDatas.TryGetValue(atlas.Category, out ChunkMeshData meshData))
                        {
                            meshData = new ChunkMeshData(atlas);
                            _meshDatas[atlas.Category] = meshData;
                        }

                        int quadIndex = meshData.AddQuadAt(index, layer.Id, layer.Behind);

                        if (!_blockIndexes.TryGetValue(index, out var list))
                        {
                            list = new List<RenderInfo>();
                            _blockIndexes[index] = list;
                        }

                        list.Add(new RenderInfo
                        {
                            MeshCategory = atlas.Category,
                            QuadIndex = quadIndex,
                            BlockId = layer.Id,
                            Behind = layer.Behind
                        });
                    }
                }

            return this;
        }

        public void ApplyMesh(ChunkMeshData meshData)
        {
            if (meshData.TryApplyMesh(out GameObject gameObject))
            {
                gameObject.transform.SetParent(_categoriesParent, false);
                _categoryObjects.Add(gameObject);
                _refresher.ScheduleRefresh(meshData);
            }
        }
        public ChunkMeshBuilder ApplyMesh()
        {
            foreach (var meshData in _meshDatas.Values)
                ApplyMesh(meshData);

            return this;
        }

        public void DrawBlock(BlockIndex index)
        {
            var layers = _chunk.Render.GetRenderStack(index, _blockDatabase, _blockAtlasDatabase);

            if (!_blockIndexes.TryGetValue(index, out var existing))
            {
                _DrawBlockStack(index, layers);
                return;
            }

            _EraseBlockStack(index);
            _DrawBlockStack(index, layers);
        }

        public void EraseBlock(BlockIndex index)
        {
            if (!_blockIndexes.TryGetValue(index, out var existing)) return;

            var layers = _chunk.Render.GetRenderStack(index, _blockDatabase, _blockAtlasDatabase);

            _EraseBlockStack(index);

            if (layers != null && layers.Count > 0)
                _DrawBlockStack(index, layers);
        }

        private void _DrawBlockStack(BlockIndex index, List<RenderLayer> layers)
        {
            if (layers == null || layers.Count == 0) return;

            if (!_blockIndexes.TryGetValue(index, out var list))
            {
                list = new List<RenderInfo>();
                _blockIndexes[index] = list;
            }

            var changed = new HashSet<ChunkMeshData>();

            foreach (var layer in layers)
            {
                BlockInfo info = _blockDatabase.Get(layer.Id);
                BlockAtlasInfo atlas = _blockAtlasDatabase.Get(info.AtlasCategory);

                if (!_meshDatas.TryGetValue(atlas.Category, out ChunkMeshData meshData))
                {
                    meshData = new ChunkMeshData(atlas);
                    ApplyMesh(meshData);
                    _meshDatas[atlas.Category] = meshData;
                }

                int quadIndex = meshData.AddQuadAt(index, layer.Id, layer.Behind);

                list.Add(new RenderInfo
                {
                    MeshCategory = atlas.Category,
                    QuadIndex = quadIndex,
                    BlockId = layer.Id,
                    Behind = layer.Behind
                });

                changed.Add(meshData);
            }

            foreach (var md in changed) _refresher.ScheduleRefresh(md);
        }

        private void _EraseBlockStack(BlockIndex index)
        {
            if (!_blockIndexes.TryGetValue(index, out var list) || list == null || list.Count == 0)
                return;

            var affected = new HashSet<ChunkMeshData>();

            var toRemove = list.Select(r => r.QuadIndex).OrderByDescending(i => i).ToList();

            foreach (var quadIndex in toRemove)
            {
                var renderInfo = list.FirstOrDefault(r => r.QuadIndex == quadIndex);
                if (renderInfo.QuadIndex != quadIndex) continue;

                if (!_meshDatas.TryGetValue(renderInfo.MeshCategory, out var meshData))
                    continue;

                int last = meshData.QuadCount - 1;
                BlockIndex? moved = meshData.RemoveQuadAt(quadIndex);

                affected.Add(meshData);

                if (moved.HasValue)
                {
                    var movedCoord = moved.Value;
                    if (_blockIndexes.TryGetValue(movedCoord, out var movedList))
                    {
                        for (int i = 0; i < movedList.Count; i++)
                        {
                            if (movedList[i].QuadIndex == last)
                            {
                                var ri = movedList[i];
                                ri.QuadIndex = quadIndex;
                                movedList[i] = ri;
                                break;
                            }
                        }
                    }
                }

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].QuadIndex == quadIndex)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            }

            if (list.Count == 0)
                _blockIndexes.Remove(index);

            foreach (var md in affected) _refresher.ScheduleRefresh(md);
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
            if (_chunk == null) return;
            _chunk.Events.OnBlockSet += OnChunkBlockSet;
            _chunk.Events.OnBlockBroken += OnChunkBlockBroken;
        }
        private void UnsubscribeFromChunkChanges()
        {
            if (_chunk == null) return;
            _chunk.Events.OnBlockSet -= OnChunkBlockSet;
            _chunk.Events.OnBlockBroken -= OnChunkBlockBroken;
        }

        public void ClearAll()
        {
            _refresher.UnscheduleAll();
            _categoryObjects.ForEach(Object.DestroyImmediate);
            _categoryObjects.Clear();
            _blockIndexes.Clear();
            _meshDatas.Clear();
        }
        public void Dispose()
        {
            UnsubscribeFromChunkChanges();
            _refresher.UnscheduleAll();
            ClearAll();
        }
    }
}