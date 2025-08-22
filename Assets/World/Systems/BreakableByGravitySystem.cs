using World.Blocks;
using World.Chunks;

namespace World.Systems
{
    public class BreakableByGravitySystem : IWorldSystem
    {
        private void BlockBreakMatcher(WorldPosition position, Block block, BlockLayer layer)
        {

        }
        public void RegisterSystem(ChunksManager manager)
        {
            manager.Blocks.Events.OnBlockBroken += BlockBreakMatcher;
        }
        public void UnregisterSystem(ChunksManager manager)
        {
            manager.Blocks.Events.OnBlockBroken -= BlockBreakMatcher;
        }
    }
}