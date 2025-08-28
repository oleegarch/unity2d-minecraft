using System;
using World.Blocks;
using World.Inventories;

namespace World.Chunks
{
    public class ChunkBlockEvents
    {
        public event Action<BlockIndex, Block, BlockLayer> OnBlockSet;
        public event Action<BlockIndex, Block, BlockLayer> OnBlockBroken;
        public event Action<BlockIndex, Block, BlockLayer> OnBlockSetByPlayer;
        public event Action<BlockIndex, Block, BlockLayer> OnBlockBrokenByPlayer;
        public event Action<BlockIndex, IInventory, BlockLayer> OnBlockInventoryDropped;

        public void InvokeBlockSet(BlockIndex index, Block block, BlockLayer layer, BlockUpdateSource source)
        {
            if (source == BlockUpdateSource.Player)
                OnBlockSetByPlayer?.Invoke(index, block, layer);

            OnBlockSet?.Invoke(index, block, layer);
        }

        public void InvokeBlockBroken(BlockIndex index, Block block, BlockLayer layer, BlockUpdateSource source)
        {
            if (source == BlockUpdateSource.Player)
                OnBlockBrokenByPlayer?.Invoke(index, block, layer);

            OnBlockBroken?.Invoke(index, block, layer);
        }

        public void InvokeBlockInventoryDropped(BlockIndex index, IInventory inventory, BlockLayer layer)
        {
            OnBlockInventoryDropped?.Invoke(index, inventory, layer);
        }
    }
}