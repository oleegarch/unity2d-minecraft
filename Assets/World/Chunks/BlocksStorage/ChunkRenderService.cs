using System.Collections.Generic;
using World.Blocks;

namespace World.Chunks.BlocksStorage
{
    // Интерфейс для логики, связанной с визуальной информацией
    public interface IChunkRenderService
    {
        public bool TryGetBlockRenderId(BlockIndex index, out ushort renderId, out bool isBehind);
        public void DisableDarkening(BlockIndex index);
    }
    public class ChunkRenderService : IChunkRenderService
    {
        private readonly HashSet<BlockIndex> _noDarkeningOverrides = new();
        private readonly IChunkBlockAccessor _blocks;

        public ChunkRenderService(IChunkBlockAccessor blocks)
        {
            _blocks = blocks;
        }

        public bool TryGetBlockRenderId(BlockIndex index, out ushort renderId, out bool isBehind)
        {
            Block main = _blocks.Get(index, BlockLayer.Main);
            renderId = main.Id;
            isBehind = main.IsAir();

            if (isBehind)
            {
                Block behind = _blocks.Get(index, BlockLayer.Behind);
                if (behind.IsAir())
                {
                    return false;
                }
                isBehind = ShouldDarken(index);
                renderId = behind.Id;
            }

            return true;
        }

        public void DisableDarkening(BlockIndex index) => _noDarkeningOverrides.Add(index);
        public bool ShouldDarken(BlockIndex index) => !_noDarkeningOverrides.Contains(index);
    }
}