using System.Collections.Generic;
using World.Blocks;

namespace World.Chunks.Blocks.Storages
{
    public class SparseBlockStorage : IBlockLayerStorage
    {
        private readonly Dictionary<BlockIndex, Block> _blocks = new();

        public Block Get(BlockIndex index)
        {
            if (_blocks.TryGetValue(index, out var block))
            {
                return block;
            }

            return Block.Air;
        }
        public void Set(BlockIndex index, Block block)
        {
            if (block.IsAir)
            {
                _blocks.Remove(index);
            }
            else
            {
                _blocks[index] = block;
            }
        }
    }
}