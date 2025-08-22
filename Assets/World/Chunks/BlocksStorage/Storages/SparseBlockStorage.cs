using System.Collections.Generic;
using World.Blocks;

namespace World.Chunks.BlocksStorage.Storages
{
    public class SparseBlockStorage : IBlockLayerStorage
    {
        private readonly Dictionary<BlockIndex, Block> _blocks = new();

        public Block Get(BlockIndex index) => _blocks.TryGetValue(index, out var block) ? block : Block.Air;
        public void Set(BlockIndex index, Block block) => _blocks[index] = block;
    }
}