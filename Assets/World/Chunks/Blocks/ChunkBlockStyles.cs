using System;
using System.Collections.Generic;
using World.Blocks;

namespace World.Chunks.Blocks
{
    // IChunkBlockModifierWithStyles — модификация блоков по слоям как в IChunkBlockModifier, но с перезаписью стилей
    public interface IChunkBlockModifierWithStyles
    {
        public void Set(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer);
        public void SetSilent(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer);
        public bool TrySet(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer);
    }
    // Интерфейс для логики, связанной с визуальной информацией
    public interface IChunkBlockStyles : IChunkBlockModifierWithStyles, IDisposable
    {
        public BlockStyles GetBlockStyles(BlockIndex index, BlockLayer layer);
        public void OverrideBlockStyles(BlockIndex index, BlockStyles styles, BlockLayer layer);
        public bool RemoveOverrideBlockStyles(BlockIndex index, BlockLayer layer);
    }
    public class ChunkBlockStyles : IChunkBlockStyles
    {
        private readonly Dictionary<BlockLayer, Dictionary<BlockIndex, BlockStyles>> _styleOverrides = new();
        private readonly IChunkBlockModifier _blocks;
        private readonly ChunkBlockEvents _events;

        public ChunkBlockStyles(ChunkBlockEvents events, IChunkBlockModifier blocks)
        {
            _events = events;
            _events.OnBlockBroken += HandleBlockBroken;
            _blocks = blocks;
        }

        public bool EnsureValidStylesOverrideAttempt(BlockIndex index, BlockStyles overrided, BlockLayer layer)
        {
            // Нельзя ставить Behind блок с IsBehind==false на Main блок
            return !(
                layer == BlockLayer.Behind &&
                overrided.IsBehind == false &&
                !_blocks.Get(index, BlockLayer.Main).IsAir
            );
        }
        public void Set(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer)
        {
            if (!EnsureValidStylesOverrideAttempt(index, overrided, layer))
                throw new InvalidOperationException("Can't set Layer=Behind+IsBehind=false blocks if the Main block exists.");
    
            OverrideBlockStyles(index, overrided, layer);
            _blocks.Set(index, block, layer);
        }
        public void SetSilent(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer)
        {
            if (!EnsureValidStylesOverrideAttempt(index, overrided, layer))
                throw new InvalidOperationException("Can't set Layer=Behind+IsBehind=false blocks if the Main block exists.");
                
            OverrideBlockStyles(index, overrided, layer);
            _blocks.SetSilent(index, block, layer);
        }
        public bool TrySet(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer)
        {
            if (_blocks.Get(index, layer).IsAir)
            {
                if (!EnsureValidStylesOverrideAttempt(index, overrided, layer))
                    return false;

                OverrideBlockStyles(index, overrided, layer);
                _blocks.Set(index, block, layer);
                
                return true;
            }
            return false;
        }

        public BlockStyles GetBlockStyles(BlockIndex index, BlockLayer layer)
        {
            if (!_styleOverrides.TryGetValue(layer, out var overrides) || !overrides.ContainsKey(index))
                return BlockStyles.ByLayer[(int)layer];

            return overrides[index];
        }
        public void OverrideBlockStyles(BlockIndex index, BlockStyles styles, BlockLayer layer)
        {
            // если в аргументе стилей такие же стили как дефолтные этого слоя — попытаемся удалить их вовсе
            // потому что они итак возвращаются по умолчанию при отсутствии перезаписанных стилей
            if (styles == BlockStyles.ByLayer[(int)layer])
            {
                RemoveOverrideBlockStyles(index, layer);
                return;
            }

            if (!_styleOverrides.TryGetValue(layer, out var overrides))
                overrides = _styleOverrides[layer] = new();

            overrides[index] = styles;
            _events.InvokeBlockStylesCreated(index, styles, layer);
        }
        public bool RemoveOverrideBlockStyles(BlockIndex index, BlockLayer layer)
        {
            if (!_styleOverrides.TryGetValue(layer, out var overrides) || !overrides.ContainsKey(index))
                return false;

            _events.InvokeBlockStylesRemoved(index, overrides[index], layer);
            overrides.Remove(index);

            return true;
        }

        private void HandleBlockBroken(BlockIndex index, Block block, BlockLayer layer)
        {
            RemoveOverrideBlockStyles(index, layer);
        }
        public void Dispose()
        {
            _events.OnBlockBroken -= HandleBlockBroken;
        }
    }
}