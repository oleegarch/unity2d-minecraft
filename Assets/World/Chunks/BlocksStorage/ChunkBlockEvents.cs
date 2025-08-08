using System;
using World.Blocks;

namespace World.Chunks
{
    public class ChunkBlockEvents
    {
        public event Action<BlockIndex, Block, BlockLayer> OnBlockSet;
        public event Action<BlockIndex, Block, BlockLayer> OnBlockBroken;

        public void InvokeBlockSet(BlockIndex index, Block block, BlockLayer layer)
            => OnBlockSet?.Invoke(index, block, layer);

        public void InvokeBlockBroken(BlockIndex index, Block block, BlockLayer layer)
            => OnBlockBroken?.Invoke(index, block, layer);
    }
}