using World.Blocks;
using World.Chunks.BlocksStorage;
using World.Chunks.Generator.Steps;

namespace World.Chunks.Generator.Procedural
{
    public class ChunkCreationStep : IChunkCreationStep
    {
        private readonly IProceduralBlockProvider _provider;

        public ChunkCreationStep(IProceduralBlockProvider provider)
            => _provider = provider;

        public Chunk Execute(ChunkIndex chunkIndex, byte size)
        {
            Chunk chunk = new Chunk(chunkIndex, size);
            
            int worldChunkX = chunkIndex.x * size;
            int worldChunkY = chunkIndex.y * size;

            // Procedural generation: blocks (surface, depth, caves, ores)
            for (byte x = 0; x < size; x++)
                for (byte y = 0; y < size; y++)
                {
                    int worldX = worldChunkX + x;
                    int worldY = worldChunkY + y;
                    var (mainId, behindId) = _provider.GenerateBlock(worldX, worldY);
                    chunk.Blocks.SetSilent(new BlockIndex(x, y), new Block(mainId), BlockLayer.Main);
                    chunk.Blocks.SetSilent(new BlockIndex(x, y), new Block(behindId), BlockLayer.Behind);
                }

            return chunk;
        }
    }
}