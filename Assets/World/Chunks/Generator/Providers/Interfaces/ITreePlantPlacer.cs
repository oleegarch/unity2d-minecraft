using World.Chunks.BlocksStorage;

namespace World.Chunks.Generator.Providers
{
    public interface IPlantPlacer
    {
        public void PlacePlants(Chunk chunk);
    }
}