using System;
using World.Blocks;
using World.Chunks.BlocksStorage.Storages;

namespace World.Chunks.BlocksStorage
{
    public class Chunk : IDisposable
    {
        public readonly ChunkIndex Index;
        public readonly byte Size;

        public readonly IChunkBlockModifier Blocks;
        public readonly IChunkRenderService Render;

        public Chunk(ChunkIndex index, byte size)
        {
            Index = index;
            Size = size;

            Blocks = new ChunkBlockModifier(new IBlockLayerStorage[]
            {
                new ArrayBlockStorage(size),  // BlockLayer.Main
                new ArrayBlockStorage(size),  // BlockLayer.Behind
                new SparseBlockStorage()      // BlockLayer.Front
            });
            Render = new ChunkRenderService(Blocks);
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
        }
    }
}