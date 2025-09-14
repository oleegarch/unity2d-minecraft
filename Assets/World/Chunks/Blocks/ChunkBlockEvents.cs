using System;
using World.Blocks;
using World.Inventories;

namespace World.Chunks
{
    public class ChunkBlockEvents
    {
        // BLOCKS
        public event Action<BlockIndex, Block, BlockLayer> OnBlockSet;
        public event Action<BlockIndex, Block, BlockLayer> OnBlockSetByPlayer;
        public event Action<BlockIndex, Block, BlockLayer> OnBlockBroken;
        public event Action<BlockIndex, Block, BlockLayer> OnBlockBrokenByPlayer;
        internal void InvokeBlockSet(BlockIndex index, Block block, BlockLayer layer, BlockUpdateSource source)
        {
            if (source == BlockUpdateSource.Player)
                OnBlockSetByPlayer?.Invoke(index, block, layer);

            OnBlockSet?.Invoke(index, block, layer);
        }
        internal void InvokeBlockBroken(BlockIndex index, Block oldBlock, BlockLayer layer, BlockUpdateSource source)
        {
            if (source == BlockUpdateSource.Player)
                OnBlockBrokenByPlayer?.Invoke(index, oldBlock, layer);

            OnBlockBroken?.Invoke(index, oldBlock, layer);
        }

        // BLOCK STYLES
        public event Action<BlockIndex, BlockStyles, BlockLayer> OnBlockStylesCreated;
        public event Action<BlockIndex, BlockStyles, BlockLayer> OnBlockStylesRemoved;
        internal void InvokeBlockStylesCreated(BlockIndex index, BlockStyles styles, BlockLayer layer)
        {
            OnBlockStylesCreated?.Invoke(index, styles, layer);
        }
        internal void InvokeBlockStylesRemoved(BlockIndex index, BlockStyles oldStyles, BlockLayer layer)
        {
            OnBlockStylesRemoved?.Invoke(index, oldStyles, layer);
        }

        // INVENTORIES
        public event Action<BlockIndex, BlockInventory, BlockLayer> OnBlockInventoryCreated;
        public event Action<BlockIndex, BlockInventory, BlockLayer> OnBlockInventoryRemoved;
        internal void InvokeBlockInventoryCreated(BlockIndex index, BlockInventory inventory, BlockLayer layer)
        {
            OnBlockInventoryCreated?.Invoke(index, inventory, layer);
        }
        internal void InvokeBlockInventoryRemoved(BlockIndex index, BlockInventory oldInventory, BlockLayer layer)
        {
            OnBlockInventoryRemoved?.Invoke(index, oldInventory, layer);
        }
    }
}