using System.Collections.Generic;
using World.Blocks;
using World.Blocks.Atlases;

namespace World.Chunks.Blocks
{
    public struct RenderLayer
    {
        public ushort Id;
        public bool Behind; // влияет на "darkness" при рендере
    }
    public interface IChunkRenderService
    {
        public bool ShouldBehind(BlockIndex index, BlockLayer layer = BlockLayer.Behind);
        public List<RenderLayer> GetRenderStack(BlockIndex index, BlockDatabase blockDatabase, BlockAtlasDatabase blockAtlasDatabase);
    }
    public class ChunkRenderService : IChunkRenderService
    {
        private readonly IChunkBlockModifier _blocks;
        private readonly IChunkBlockStyles _blockStyles;

        public ChunkRenderService(IChunkBlockModifier blocks, IChunkBlockStyles blockStyles)
        {
            _blocks = blocks;
            _blockStyles = blockStyles;
        }

        public List<RenderLayer> GetRenderStack(BlockIndex index, BlockDatabase blockDatabase, BlockAtlasDatabase blockAtlasDatabase)
        {
            var stack = new List<RenderLayer>(2);

            Block main = _blocks.Get(index, BlockLayer.Main);
            Block behind = _blocks.Get(index, BlockLayer.Behind);

            if (!behind.IsAir)
            {
                BlockInfo mainInfo = blockDatabase.Get(main.Id);
                bool renderBehind = main.IsAir || mainInfo.HasTransparentPixels;
                if (renderBehind)
                {
                    bool behindDarkness = ShouldBehind(index);
                    stack.Add(new RenderLayer { Id = behind.Id, Behind = behindDarkness });
                }
            }

            if (!main.IsAir)
                stack.Add(new RenderLayer { Id = main.Id, Behind = false });

            return stack;
        }
        public bool ShouldBehind(BlockIndex index, BlockLayer layer = BlockLayer.Behind)
        {
            return _blockStyles.GetBlockStyles(index, layer).IsBehind;
        }
    }
}