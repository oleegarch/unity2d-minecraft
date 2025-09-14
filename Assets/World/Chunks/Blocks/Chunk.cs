using System;
using World.Blocks;
using World.Chunks.Blocks.Storages;

namespace World.Chunks.Blocks
{
    public class Chunk : IDisposable
    {
        public readonly ChunkIndex Index;
        public readonly byte Size;

        public readonly ChunkBlockEvents Events;
        public readonly IChunkBlockModifier Blocks;
        public readonly IChunkBlockStyles BlockStyles;
        public readonly IChunkRenderService Render;
        public readonly IChunkBlockInventories Inventories;

        public Chunk(ChunkIndex index, byte size)
        {
            Index = index;
            Size = size;

            Events = new ChunkBlockEvents();
            Blocks = new ChunkBlockModifier(Events, new IBlockLayerStorage[]
            {
                new ArrayBlockStorage(size),  // BlockLayer.Main
                new ArrayBlockStorage(size),  // BlockLayer.Behind
                new SparseBlockStorage()      // BlockLayer.Front
            });
            BlockStyles = new ChunkBlockStyles(Events, Blocks);
            Inventories = new ChunkBlockInventories(Events);
            Render = new ChunkRenderService(Blocks, BlockStyles);
        }

        public bool TryGetBlockIndex(WorldPosition worldPosition, out BlockIndex blockIndex)
        {
            if (worldPosition.ToChunkIndex(Size) == Index)
            {
                blockIndex = worldPosition.ToBlockIndex(Size);
                return true;
            }

            blockIndex = BlockIndex.Zero;
            return false;
        }

        public void Dispose()
        {
            BlockStyles.Dispose();
            Inventories.Dispose();
        }
    }
}