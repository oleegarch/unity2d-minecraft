using System;
using System.Collections.Generic;
using World.Blocks;
using World.Blocks.Atlases;

namespace World.Chunks.BlocksStorage
{
    public struct RenderLayer
    {
        public ushort Id;
        public bool Behind; // влияет на "darkness" при рендере
    }
    // IChunkBlockPlacerWithStyles — модификация блоков по слоям как в IChunkBlockModifier, но с перезаписью стилей
    public interface IChunkBlockPlacerWithStyles
    {
        public void Set(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer);
        public bool TrySet(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer);
    }
    // Интерфейс для логики, связанной с визуальной информацией
    public interface IChunkRenderService : IChunkBlockPlacerWithStyles
    {
        public List<RenderLayer> GetRenderStack(BlockIndex index, BlockDatabase blockDatabase, BlockAtlasDatabase blockAtlasDatabase);
        public BlockStyles GetBlockStyles(BlockIndex index, BlockLayer layer);
        public void OverrideBlockStyles(BlockIndex index, BlockStyles styles, BlockLayer layer);
        public bool RemoveOverrideBlockStyles(BlockIndex index, BlockLayer layer);
        public bool ShouldBehind(BlockIndex index, BlockLayer layer = BlockLayer.Behind);
        public void Dispose();
    }
    public class ChunkRenderService : IChunkRenderService, IDisposable
    {
        private readonly Dictionary<BlockLayer, Dictionary<BlockIndex, BlockStyles>> _styleOverrides = new();
        private readonly IChunkBlockModifier _blocks;

        public ChunkRenderService(IChunkBlockModifier blocks)
        {
            _blocks = blocks;
            _blocks.Events.OnBlockBroken += HandleBlockBroken;
        }

        public void Set(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer)
        {
            OverrideBlockStyles(index, overrided, layer);
            _blocks.Set(index, block, layer);
        }
        public bool TrySet(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer)
        {
            if (_blocks.Get(index, layer).IsAir)
            {
                OverrideBlockStyles(index, overrided, layer);
                _blocks.Set(index, block, layer);
                return true;
            }
            return false;
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
            {
                stack.Add(new RenderLayer { Id = main.Id, Behind = false });
            }

            return stack;
        }

        public BlockStyles GetDefaultBlockStyles(BlockLayer layer)
        {
            return BlockStyles.ByLayer[(int)layer];
        }
        public BlockStyles GetBlockStyles(BlockIndex index, BlockLayer layer)
        {
            if (!_styleOverrides.TryGetValue(layer, out var overrides) || !overrides.ContainsKey(index))
                return GetDefaultBlockStyles(layer);

            return overrides[index];
        }
        public void OverrideBlockStyles(BlockIndex index, BlockStyles styles, BlockLayer layer)
        {
            if (styles == GetDefaultBlockStyles(layer))
                return;

            if (!_styleOverrides.TryGetValue(layer, out var overrides))
                overrides = _styleOverrides[layer] = new();

            overrides[index] = styles;
        }
        public bool RemoveOverrideBlockStyles(BlockIndex index, BlockLayer layer)
        {
            if (!_styleOverrides.TryGetValue(layer, out var overrides) || !overrides.ContainsKey(index))
                return false;

            overrides.Remove(index);

            return true;
        }
        public bool ShouldBehind(BlockIndex index, BlockLayer layer = BlockLayer.Behind)
        {
            if (!_styleOverrides.TryGetValue(layer, out var overrides) || !overrides.ContainsKey(index))
                return GetDefaultBlockStyles(layer).IsBehind;

            return overrides[index].IsBehind;
        }

        private void HandleBlockBroken(BlockIndex index, Block block, BlockLayer layer)
        {
            RemoveOverrideBlockStyles(index, layer);
        }
        public void Dispose()
        {
            _blocks.Events.OnBlockBroken -= HandleBlockBroken;
        }
    }
}