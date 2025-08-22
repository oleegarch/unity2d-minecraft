using System;
using World.Blocks;

namespace World.Chunks
{
    public class ChunkBlockEvents
    {
        public event Action<BlockIndex, Block, BlockLayer> OnBlockSet;
        public event Action<BlockIndex, Block, BlockLayer> OnBlockBroken;
        public event Action<BlockIndex, Block, BlockLayer> OnBlockSetByPlayer;
        public event Action<BlockIndex, Block, BlockLayer> OnBlockBrokenByPlayer;

        public void InvokeBlockSet(BlockIndex index, Block block, BlockLayer layer, BlockBrokeSource source)
        {
            if (source == BlockBrokeSource.Player)
                OnBlockSetByPlayer?.Invoke(index, block, layer);

            OnBlockSet?.Invoke(index, block, layer);
        }

        public void InvokeBlockBroken(BlockIndex index, Block block, BlockLayer layer, BlockBrokeSource source)
        {
            if (source == BlockBrokeSource.Player)
                OnBlockBrokenByPlayer?.Invoke(index, block, layer);
                
            OnBlockBroken?.Invoke(index, block, layer);
        }
    }
}