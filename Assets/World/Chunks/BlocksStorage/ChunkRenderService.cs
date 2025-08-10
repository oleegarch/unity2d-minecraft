using System;
using System.Collections.Generic;
using World.Blocks;

namespace World.Chunks.BlocksStorage
{
    // Интерфейс для логики, связанной с визуальной информацией
    public interface IChunkRenderService
    {
        public bool TryGetBlockRenderId(BlockIndex index, out ushort renderId, out bool isBehind);
        public void OverrideBlockStyles(BlockIndex index, BlockStyles styles, BlockLayer layer);
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
                isBehind = ShouldBehind(index);
                renderId = behind.Id;
            }

            return true;
        }

        public void OverrideBlockStyles(BlockIndex index, BlockStyles styles, BlockLayer layer)
        {
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
            var defaultBehind = BlockStyles.ByLayer[(int)layer].IsBehind;
            var overrides = _styleOverrides[layer];
            if (overrides == null) return defaultBehind;

            if (!overrides.ContainsKey(index)) return defaultBehind;

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