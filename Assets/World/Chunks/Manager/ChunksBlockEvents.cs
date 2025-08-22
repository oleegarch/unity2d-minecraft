using System;
using World.Blocks;

namespace World.Chunks
{
    public class ChunksBlockEvents
    {
        public event Action<WorldPosition, Block, BlockLayer> OnBlockSet;
        public event Action<WorldPosition, Block, BlockLayer> OnBlockBroken;
        public event Action<WorldPosition, Block, BlockLayer> OnBlockSetByPlayer;
        public event Action<WorldPosition, Block, BlockLayer> OnBlockBrokenByPlayer;

        public void InvokeBlockSet(WorldPosition position, Block block, BlockLayer layer, BlockBrokeSource source)
        {
            if (source == BlockBrokeSource.Player)
                OnBlockSetByPlayer?.Invoke(position, block, layer);

            OnBlockSet?.Invoke(position, block, layer);
        }

        public void InvokeBlockBroken(WorldPosition position, Block block, BlockLayer layer, BlockBrokeSource source)
        {
            if (source == BlockBrokeSource.Player)
                OnBlockBrokenByPlayer?.Invoke(position, block, layer);
                
            OnBlockBroken?.Invoke(position, block, layer);
        }
    }
}