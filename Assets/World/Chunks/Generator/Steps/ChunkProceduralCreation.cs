using World.Blocks;
using World.Chunks.BlocksStorage;
using World.Chunks.BlocksStorage.Storages;
using World.Chunks.Generator.Providers;

namespace World.Chunks.Generator.Steps
{
    public class ChunkProceduralCreation : IChunkCreationStep
    {
        private readonly IProceduralBlockProvider _provider;

        public ChunkProceduralCreation(IProceduralBlockProvider provider)
            => _provider = provider;

        public Chunk Execute(ChunkIndex chunkIndex, byte size, int seed)
        {
            Chunk chunk = new Chunk(chunkIndex, size);

            IBlockLayerStorage mainLayer = chunk.Blocks.GetLayer(BlockLayer.Main);
            IBlockLayerStorage behindLayer = chunk.Blocks.GetLayer(BlockLayer.Behind);
            
            int worldChunkX = chunkIndex.x * size;
            int worldChunkY = chunkIndex.y * size;

            // Procedural generation: blocks (surface, depth, caves, ores)
            for (byte x = 0; x < size; x++)
                for (byte y = 0; y < size; y++)
                {
                    int worldX = worldChunkX + x;
                    int worldY = worldChunkY + y;
                    var (mainId, behindId) = _provider.GenerateBlock(worldX, worldY, seed);
                    mainLayer.Set(x, y, mainId);
                    behindLayer.Set(x, y, behindId);
                }

            return chunk;
        }
    }
}