using System;
using System.Collections.Generic;
using R3;
using World.Blocks;

namespace World.Chunks.Blocks
{
    #region Инт. Блоков и стилей
    // IChunkBlockModifierWithStyles — модификация блоков по слоям как в IChunkBlockModifier, но ещё с перезаписью стилей
    public interface IChunkBlockModifierWithStyles
    {
        /// <summary>Установить блок+стили с событием.</summary>
        public void Set(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer);
        /// <summary>Установить блок+стили без событий.</summary>
        public void SetSilent(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer);
        /// <summary>Если блок != воздух, устанавливаем его и стили и возвращаем true.</summary>
        public bool TrySet(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer);
    }
    #endregion

    #region Инт. только стилей
    // Интерфейс для логики, связанной с визуальной информацией
    public interface IChunkBlockStyles : IChunkBlockModifierWithStyles, IDisposable
    {
        /// <summary>Получить стили блока</summary>
        public BlockStyles GetBlockStyles(BlockIndex index, BlockLayer layer);
        /// <summary>Перезаписывает стили блока на определённом слое с дефолтных на кастомные</summary>
        public void OverrideBlockStyles(BlockIndex index, BlockStyles styles, BlockLayer layer);
        public void OverrideBlockStylesSilent(BlockIndex index, BlockStyles styles, BlockLayer layer); // то же самое, но без событий
        /// <summary>Удалить кастомные стили блока</summary>
        public bool RemoveOverrideBlockStyles(BlockIndex index, BlockLayer layer);
        public bool RemoveOverrideBlockStylesSilent(BlockIndex index, BlockLayer layer); // то же самое, но без событий
    }
    #endregion

    public class ChunkBlockStyles : IChunkBlockStyles
    {
        #region Конструктор
        private readonly Dictionary<BlockLayer, Dictionary<BlockIndex, BlockStyles>> _styleOverrides = new();
        private readonly IChunkBlockModifier _blocks;
        private readonly ChunkBlockEvents _events;
        private readonly IDisposable _blockBrokenSub;

        public ChunkBlockStyles(ChunkBlockEvents events, IChunkBlockModifier blocks)
        {
            _events = events;

            _blockBrokenSub = _events.BlockBroken.Subscribe(be =>
            {
                RemoveOverrideBlockStyles(be.Index, be.Layer);
            });

            _blocks = blocks;
        }
        #endregion

        #region Валидация
        public bool EnsureValidStylesOverrideAttempt(BlockIndex index, BlockStyles overrided, BlockLayer layer)
        {
            // Нельзя ставить Behind блок с IsBehind==false на Main блок
            return !(
                layer == BlockLayer.Behind &&
                overrided.IsBehind == false &&
                !_blocks.Get(index, BlockLayer.Main).IsAir
            );
        }
        #endregion

        #region Блоки+стили
        public void SetSilent(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer)
        {
            if (!EnsureValidStylesOverrideAttempt(index, overrided, layer))
                throw new InvalidOperationException("Can't set Layer=Behind+IsBehind=false blocks if the Main block exists.");

            OverrideBlockStylesSilent(index, overrided, layer);
            _blocks.SetSilent(index, block, layer);
        }
        public void Set(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer)
        {
            if (!EnsureValidStylesOverrideAttempt(index, overrided, layer))
                throw new InvalidOperationException("Can't set Layer=Behind+IsBehind=false blocks if the Main block exists.");

            _Set(index, block, overrided, layer);
        }
        private void _Set(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer)
        {
            // если блок != воздух, только тогда переопределяем стили блока.
            // потому что в случае удаления блока стили попутно удалятся в событии HandleBlockBroken
            if (!block.IsAir)
                OverrideBlockStyles(index, overrided, layer);

            _blocks.Set(index, block, layer);
        }
        public bool TrySet(BlockIndex index, Block block, BlockStyles overrided, BlockLayer layer)
        {
            if (_blocks.Get(index, layer).IsAir)
            {
                if (!EnsureValidStylesOverrideAttempt(index, overrided, layer))
                    return false;

                _Set(index, block, overrided, layer);

                return true;
            }

            return false;
        }
        #endregion

        #region Только стили
        // получение стилей
        public BlockStyles GetBlockStyles(BlockIndex index, BlockLayer layer)
        {
            if (!_styleOverrides.TryGetValue(layer, out var overrides) || !overrides.ContainsKey(index))
                return BlockStyles.ByLayer[(int)layer];

            return overrides[index];
        }
        // перезапись с событиями
        public void OverrideBlockStyles(BlockIndex index, BlockStyles styles, BlockLayer layer)
        {
            if (_OverrideBlockStylesSilent(index, styles, layer, out bool removed, out BlockStyles oldStyles))
            {
                _events.InvokeBlockStylesCreated(index, styles, layer);
            }
            else
            {
                if (removed)
                {
                    _events.InvokeBlockStylesRemoved(index, oldStyles, layer);
                }
            }
        }
        // перезапись без событий
        public void OverrideBlockStylesSilent(BlockIndex index, BlockStyles styles, BlockLayer layer)
        {
            _OverrideBlockStylesSilent(index, styles, layer, out bool removed, out BlockStyles oldStyles);
        }

        // удаление с событиями
        public bool RemoveOverrideBlockStyles(BlockIndex index, BlockLayer layer)
        {
            if (_RemoveOverrideBlockStylesSilent(index, layer, out BlockStyles oldStyles))
            {
                _events.InvokeBlockStylesRemoved(index, oldStyles, layer);
                return true;
            }

            return false;
        }
        // удаление без событий
        public bool RemoveOverrideBlockStylesSilent(BlockIndex index, BlockLayer layer)
        {
            return _RemoveOverrideBlockStylesSilent(index, layer, out BlockStyles oldStyles);
        }
        #endregion

        #region Сама реализация
        private bool _OverrideBlockStylesSilent(BlockIndex index, BlockStyles styles, BlockLayer layer, out bool removed, out BlockStyles oldStyles)
        {
            // если в аргументе стилей такие же стили как дефолтные этого слоя — попытаемся удалить их вовсе
            // потому что они итак возвращаются по умолчанию при отсутствии перезаписанных стилей
            if (styles == BlockStyles.ByLayer[(int)layer])
            {
                removed = _RemoveOverrideBlockStylesSilent(index, layer, out oldStyles);
                return false;
            }

            if (!_styleOverrides.TryGetValue(layer, out var overrides))
                overrides = _styleOverrides[layer] = new();

            overrides[index] = styles;
            removed = false;
            oldStyles = BlockStyles.ByLayer[(int)layer];
            return true;
        }
        private bool _RemoveOverrideBlockStylesSilent(BlockIndex index, BlockLayer layer, out BlockStyles oldStyles)
        {
            if (!_styleOverrides.TryGetValue(layer, out var overrides) || !overrides.ContainsKey(index))
            {
                oldStyles = BlockStyles.ByLayer[(int)layer];
                return false;
            }

            oldStyles = overrides[index];
            overrides.Remove(index);

            return true;
        }
        #endregion

        #region Очистка
        public void Dispose()
        {
            _blockBrokenSub.Dispose();
        }
        #endregion
    }
}