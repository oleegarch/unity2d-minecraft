using System.Collections.Generic;
using World.Blocks;

namespace World.Chunks.BlocksStorage
{
    public class ChunkRenderData
    {
        private readonly HashSet<BlockIndex> _noDarkeningOverrides = new();

        public void DisableDarkening(BlockIndex index) => _noDarkeningOverrides.Add(index);
        public bool ShouldDarken(BlockIndex index) => !_noDarkeningOverrides.Contains(index);
    }
}