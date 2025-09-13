using System.Collections.Generic;
using World.Blocks;
using World.Chunks;
using World.Chunks.Blocks;
using World.Chunks.Blocks.Storages;
using World.Inventories;

namespace World.Storage
{
    public class ChunkDiff
    {
        public readonly ChunkIndex Index;
        public readonly byte Size;
        public readonly IBlockLayerStorage[] ModifiedBlockLayers;
        public readonly Dictionary<BlockLayer, Dictionary<BlockIndex, BlockStyles>> ModifiedBlockStyleOverrides;
        public readonly Dictionary<BlockLayer, Dictionary<BlockIndex, BlockInventory>> ModifiedInventoriesByLayer;

        public ChunkDiff(Chunk chunk)
        {
            Index = chunk.Index;
            Size = chunk.Size;

            ModifiedBlockLayers = new IBlockLayerStorage[]
            {
                new SparseBlockStorage(),  // BlockLayer.Main
                new SparseBlockStorage(),  // BlockLayer.Behind
                new SparseBlockStorage()   // BlockLayer.Front
            };
            ModifiedBlockStyleOverrides = new();
            ModifiedInventoriesByLayer = new();
        }
    }
}