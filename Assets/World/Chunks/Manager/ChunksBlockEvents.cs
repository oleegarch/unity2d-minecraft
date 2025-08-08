using System;
using World.Blocks;

namespace World.Chunks
{
    public class ChunksBlockEvents
    {
        public event Action<WorldPosition, Block, BlockLayer> OnBlockSet;
        public event Action<WorldPosition, Block, BlockLayer> OnBlockBroken;

        public void InvokeBlockSet(WorldPosition position, Block block, BlockLayer layer)
            => OnBlockSet?.Invoke(position, block, layer);

        public void InvokeBlockBroken(WorldPosition position, Block block, BlockLayer layer)
            => OnBlockBroken?.Invoke(position, block, layer);
    }
}