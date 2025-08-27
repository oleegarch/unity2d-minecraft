using World.Chunks.BlocksStorage;

namespace World.Chunks.Generator.Steps
{
    public interface IChunkPostStep
    {
        public void Execute(Chunk chunk);
    }
}