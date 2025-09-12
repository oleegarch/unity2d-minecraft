using System.Collections.Generic;
using World.Blocks;
using World.Chunks;

namespace World.Storage
{
    public class ChunksStorage
    {
        private WorldBlockEvents _worldEvents;
        private Dictionary<ChunkIndex, ChunkDiff> _modifiedChunks;

        public ChunksStorage(WorldBlockEvents worldEvents)
        {
            _worldEvents = worldEvents;
        }

        public void Subscribe()
        {
            _worldEvents.OnBlockSet += HandleBlockSet;
            _worldEvents.OnBlockBroken += HandleBlockBroken;
        }
        public void Unsubscribe()
        {
            _worldEvents.OnBlockSet -= HandleBlockSet;
            _worldEvents.OnBlockBroken -= HandleBlockBroken;
        }

        private void HandleBlockSet(WorldPosition worldPosition, Block block, BlockLayer layer)
        {
            
        }
        private void HandleBlockBroken(WorldPosition worldPosition, Block block, BlockLayer layer)
        {
            
        }
    }
}