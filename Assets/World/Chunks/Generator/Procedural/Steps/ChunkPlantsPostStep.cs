using System.Collections.Generic;
using World.Chunks.Blocks;
using World.Chunks.Generator.Steps;

namespace World.Chunks.Generator.Procedural
{
    public class ChunkPlantsPostStep : IChunkPostStep
    {
        private readonly IEnumerable<IPlantPlacer> _placers;

        public ChunkPlantsPostStep(IEnumerable<IPlantPlacer> placers)
            => _placers = placers;

        public void Execute(Chunk chunk)
        {
            foreach (var p in _placers)
                p.PlacePlants(chunk);
        }
    }
}