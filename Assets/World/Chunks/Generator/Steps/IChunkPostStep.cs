using World.Chunks.Blocks;

namespace World.Chunks.Generator.Steps
{
    public interface IChunkPostStep
    {
        public void Execute(Chunk chunk);
    }
}