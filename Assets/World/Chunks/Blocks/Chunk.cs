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
            Render = new ChunkRenderService(Events, Blocks);
            Inventories = new ChunkBlockInventories(Events);
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
            Render.Dispose();
            Inventories.Dispose();
        }
    }
}