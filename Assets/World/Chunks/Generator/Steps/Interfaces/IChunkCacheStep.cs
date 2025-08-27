using UnityEngine;

namespace World.Chunks.Generator.Steps
{
    public interface IChunkCacheStep
    {
        public void CacheComputation(RectInt rect);
    }
}