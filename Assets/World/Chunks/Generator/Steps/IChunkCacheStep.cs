using System.Collections.Generic;
using UnityEngine;

namespace World.Chunks.Generator.Steps
{
    public interface IChunkCacheStep
    {
        public void CacheComputation(RectInt rect);
        public void CacheComputation(HashSet<ChunkIndex> indexes);
    }
}