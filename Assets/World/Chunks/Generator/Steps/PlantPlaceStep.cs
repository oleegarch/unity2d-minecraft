using System.Collections.Generic;
using World.Chunks.BlocksStorage;
using World.Chunks.Generator.Providers;

namespace World.Chunks.Generator.Steps
{
    public class PlantPlaceStep : IChunkPostStep
    {
        private readonly IEnumerable<IPlantPlacer> _placers;

        public PlantPlaceStep(IEnumerable<IPlantPlacer> placers)
            => _placers = placers;

        public void Execute(Chunk chunk, int seed)
        {
            foreach (var p in _placers)
                p.PlacePlants(chunk, seed);
        }
    }
}