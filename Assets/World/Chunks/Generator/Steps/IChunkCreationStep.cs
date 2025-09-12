using World.Chunks.Blocks;

namespace World.Chunks.Generator.Steps
{
    public interface IChunkCreationStep
    {
        public Chunk Execute(ChunkIndex index, byte chunkSize);
    }
}