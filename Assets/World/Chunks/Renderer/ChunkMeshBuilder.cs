using UnityEngine;
using System.Collections.Generic;
using System.Linq;
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
            public ushort BlockId;
            public bool Behind;
        }

        private readonly GameObject _gameObject;
        private readonly BlockDatabase _blockDatabase;
        private readonly BlockAtlasDatabase _blockAtlasDatabase;
        private readonly List<GameObject> _categoryObjects = new();
        private readonly Dictionary<BlockAtlasCategory, ChunkMeshData> _meshDatas = new();
        private readonly Dictionary<BlockIndex, List<RenderInfo>> _blockIndexes = new();

        private Chunk _chunk;

        public ChunkMeshBuilder(GameObject gameObject, BlockDatabase blockDatabase, BlockAtlasDatabase blockAtlasDatabase)
        {
            _gameObject = gameObject;
            _blockDatabase = blockDatabase;
            _blockAtlasDatabase = blockAtlasDatabase;
        }

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
            // Получаем текущий стэк из данных чанка
            var layers = _chunk.Render.GetRenderStack(index, _blockDatabase, _blockAtlasDatabase);

            // Если нет старой записи — просто дорисовываем новый стэк
            if (!_blockIndexes.TryGetValue(index, out var existing))
            {
                var changed = _DrawBlockStack(index, layers);
                // refresh только затронутых мешей
                foreach (var md in changed) md.RefreshMesh();
                return;
            }

            // Иначе перерисовываем: простая и безопасная стратегия — удалить старый, добавить новый
            var affectedErase = _EraseBlockStack(index);
            var affectedDraw = _DrawBlockStack(index, layers);

            // Refresh всех затронутых meshData
            foreach (var md in affectedErase) md.RefreshMesh();
            foreach (var md in affectedDraw) md.RefreshMesh();
        }

        public void EraseBlock(BlockIndex index)
        {
            if (!_blockIndexes.TryGetValue(index, out var existing)) return;

            // после удаления возможно появится новый стек
            var layers = _chunk.Render.GetRenderStack(index, _blockDatabase, _blockAtlasDatabase);

            var affectedErase = _EraseBlockStack(index);

            // если сейчас есть новые слои — нарисуем их
            HashSet<ChunkMeshData> affectedDraw = new();
            if (layers != null && layers.Count > 0)
                affectedDraw = _DrawBlockStack(index, layers);

            foreach (var md in affectedErase) md.RefreshMesh();
            foreach (var md in affectedDraw) md.RefreshMesh();
        }

        // добавляет все слои (layers) на указанную координату, возвращает набор затронутых ChunkMeshData
        private HashSet<ChunkMeshData> _DrawBlockStack(BlockIndex index, List<RenderLayer> layers)
        {
            var changed = new HashSet<ChunkMeshData>();
            if (layers == null || layers.Count == 0) return changed;

            if (!_blockIndexes.TryGetValue(index, out var list))
            {
                list = new List<RenderInfo>();
                _blockIndexes[index] = list;
            }

            foreach (var layer in layers)
            {
                BlockInfo info = _blockDatabase.Get(layer.Id);
                BlockAtlasInfo atlas = _blockAtlasDatabase.Get(info.AtlasCategory);

                if (!_meshDatas.TryGetValue(atlas.Category, out ChunkMeshData meshData))
                {
                    meshData = new ChunkMeshData(atlas);
                    CreateMesh(meshData);
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

            return changed;
        }

        // удаляет все квады, соответствующие данной координате
        // возвращает набор ChunkMeshData, которые были изменены
        private HashSet<ChunkMeshData> _EraseBlockStack(BlockIndex index)
        {
            var affected = new HashSet<ChunkMeshData>();

            if (!_blockIndexes.TryGetValue(index, out var list) || list == null || list.Count == 0)
                return affected;

            // Удаляем квады в порядке убывания индекса (чтобы корректно работать со смещением "последнего в конец")
            var toRemove = list.Select(r => r.QuadIndex).OrderByDescending(i => i).ToList();

            foreach (var quadIndex in toRemove)
            {
                // нужно найти renderInfo соответствующий этому quadIndex (и удалить из списка)
                var renderInfo = list.FirstOrDefault(r => r.QuadIndex == quadIndex);
                if (renderInfo.QuadIndex != quadIndex)
                {
                    // неожиданный случай — пропускаем
                    continue;
                }

                if (!_meshDatas.TryGetValue(renderInfo.MeshCategory, out var meshData))
                    continue;

                // запомним последний (который может быть перемещён)
                int last = meshData.QuadCount - 1;
                // удаляем
                BlockIndex? moved = meshData.RemoveQuadAt(quadIndex);

                affected.Add(meshData);

                // если что-то переместилось (return moved != null), обновляем соответствующую запись в _blockIndexes
                if (moved.HasValue)
                {
                    var movedCoord = moved.Value;
                    if (_blockIndexes.TryGetValue(movedCoord, out var movedList))
                    {
                        // нашли renderInfo в списке movedCoord с QuadIndex == last и заменим на quadIndex
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

                // и удаляем соответствующий renderInfo из original list
                // (используем поиск по original QuadIndex)
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].QuadIndex == quadIndex)
                    {
                        list.RemoveAt(i);
                        break;
                    }
                }
            }

            // Если после удаления список пуст — удаляем ключ
            if (list.Count == 0)
                _blockIndexes.Remove(index);

            return affected;
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
            _chunk.Blocks.Events.OnBlockSet += OnChunkBlockSet;
            _chunk.Blocks.Events.OnBlockBroken += OnChunkBlockBroken;
        }
        private void UnsubscribeFromChunkChanges()
        {
            if (_chunk == null) return;
            _chunk.Blocks.Events.OnBlockSet -= OnChunkBlockSet;
            _chunk.Blocks.Events.OnBlockBroken -= OnChunkBlockBroken;
        }

        public void ClearAll()
        {
            _categoryObjects.ForEach(Object.DestroyImmediate);
            _categoryObjects.Clear();
            _blockIndexes.Clear();
            _meshDatas.Clear();
        }
        public void Dispose()
        {
            UnsubscribeFromChunkChanges();
            ClearAll();
        }
    }
}