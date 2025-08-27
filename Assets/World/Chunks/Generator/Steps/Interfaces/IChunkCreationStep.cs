using World.Chunks.BlocksStorage;

namespace World.Chunks.Generator.Steps
{
    public interface IChunkCreationStep
    {
        public Chunk Execute(ChunkIndex index, byte chunkSize);
    }
}