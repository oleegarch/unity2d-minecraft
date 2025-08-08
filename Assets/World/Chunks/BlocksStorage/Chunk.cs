using System.Collections.Generic;
using World.Blocks;
using World.Chunks.BlocksStorage.Storages;

namespace World.Chunks.BlocksStorage
{
    public class Chunk
    {
        public readonly ChunkIndex Index;
        public readonly byte Size;

        public readonly IChunkBlockModifier Blocks;
        public readonly IChunkRenderService Render;

        public Chunk(ChunkIndex index, byte size)
        {
            Index = index;
            Size = size;

            Blocks = new ChunkBlockModifier(new Dictionary<BlockLayer, IBlockLayerStorage>()
            {
                [BlockLayer.Main] = new ArrayBlockStorage(size),
                [BlockLayer.Behind] = new ArrayBlockStorage(size),
                [BlockLayer.Front] = new SparseBlockStorage()
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
    }

}