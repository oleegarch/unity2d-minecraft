using System;
using R3;
using World.Blocks;
using World.Inventories;

namespace World.Chunks
{
    public readonly struct BlockEvent
    {
        public readonly BlockIndex Index;
        public readonly Block Block;
        public readonly BlockLayer Layer;
        public BlockEvent(BlockIndex i, Block b, BlockLayer l) { Index = i; Block = b; Layer = l; }
    }

    public readonly struct BlockStylesEvent
    {
        public readonly BlockIndex Index;
        public readonly BlockStyles Styles;
        public readonly BlockLayer Layer;
        public BlockStylesEvent(BlockIndex i, BlockStyles s, BlockLayer l) { Index = i; Styles = s; Layer = l; }
    }

    public readonly struct BlockInventoryEvent
    {
        public readonly BlockIndex Index;
        public readonly BlockInventory Inventory;
        public readonly BlockLayer Layer;
        public BlockInventoryEvent(BlockIndex i, BlockInventory inv, BlockLayer l) { Index = i; Inventory = inv; Layer = l; }
    }

    public class ChunkBlockEvents : IDisposable
    {
        public readonly Subject<BlockEvent> BlockSet = new();
        public readonly Subject<BlockEvent> BlockSetByPlayer = new();
        public readonly Subject<BlockEvent> BlockBroken = new();
        public readonly Subject<BlockEvent> BlockBrokenByPlayer = new();

        public readonly Subject<BlockStylesEvent> BlockStylesCreated = new();
        public readonly Subject<BlockStylesEvent> BlockStylesRemoved = new();

        public readonly Subject<BlockInventoryEvent> BlockInventoryCreated = new();
        public readonly Subject<BlockInventoryEvent> BlockInventoryRemoved = new();

        internal void InvokeBlockSet(BlockIndex index, Block block, BlockLayer layer, BlockUpdateSource source)
        {
            BlockSet.OnNext(new BlockEvent(index, block, layer));
            if (source == BlockUpdateSource.Player) BlockSetByPlayer.OnNext(new BlockEvent(index, block, layer));
        }

        internal void InvokeBlockBroken(BlockIndex index, Block oldBlock, BlockLayer layer, BlockUpdateSource source)
        {
            BlockBroken.OnNext(new BlockEvent(index, oldBlock, layer));
            if (source == BlockUpdateSource.Player) BlockBrokenByPlayer.OnNext(new BlockEvent(index, oldBlock, layer));
        }

        internal void InvokeBlockStylesCreated(BlockIndex index, BlockStyles styles, BlockLayer layer)
            => BlockStylesCreated.OnNext(new BlockStylesEvent(index, styles, layer));

        internal void InvokeBlockStylesRemoved(BlockIndex index, BlockStyles oldStyles, BlockLayer layer)
            => BlockStylesRemoved.OnNext(new BlockStylesEvent(index, oldStyles, layer));

        internal void InvokeBlockInventoryCreated(BlockIndex index, BlockInventory inventory, BlockLayer layer)
            => BlockInventoryCreated.OnNext(new BlockInventoryEvent(index, inventory, layer));

        internal void InvokeBlockInventoryRemoved(BlockIndex index, BlockInventory oldInventory, BlockLayer layer)
            => BlockInventoryRemoved.OnNext(new BlockInventoryEvent(index, oldInventory, layer));

        public void Dispose()
        {
            BlockSet.Dispose();
            BlockSetByPlayer.Dispose();
            BlockBroken.Dispose();
            BlockBrokenByPlayer.Dispose();
            BlockStylesCreated.Dispose();
            BlockStylesRemoved.Dispose();
            BlockInventoryCreated.Dispose();
            BlockInventoryRemoved.Dispose();
        }
    }
}