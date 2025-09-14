using System;
using System.Collections.Generic;
using World.Blocks;
using World.Chunks.Blocks;
using World.Chunks.Blocks.Storages;
using World.Inventories;

namespace World.Chunks.Storage
{
    /// <summary>
    /// Класс хранящий изменения в Chunk.
    /// 
    /// В конструктор передаётся сам Chunk, а дальше происходит подписка на изменения,
    /// позволяющая хранить все произведённые изменения в Chunk здесь(ChunkDiff).
    /// 
    /// Изменения блоков хранятся в ModifiedBlockLayers в Sparse слоях,
    /// которые позволяют хранить конкретные изменения, а не весь чанк блоков целиком.
    /// </summary>
    [Serializable]
    public class ChunkDiff
    {
        #region Конструктор
        public readonly ChunkIndex Index;
        public readonly byte Size;
        public readonly IBlockLayerStorage[] ModifiedBlockLayers;
        public readonly Dictionary<BlockLayer, Dictionary<BlockIndex, BlockStyles>> ModifiedBlockStyleOverrides;
        public readonly Dictionary<BlockLayer, Dictionary<BlockIndex, BlockInventory>> ModifiedInventoriesByLayer;

        public event Action OnChanged;

        private Chunk _chunk;
        private bool _isApplied;
        private bool _isSubscribedToEvents;
        public bool IsApplied => _isApplied;
        public bool IsSubscribedToEvents => _isSubscribedToEvents;

        public ChunkDiff(Chunk chunk)
        {
            _chunk = chunk;
            _isApplied = false;
            _isSubscribedToEvents = false;

            Index = chunk.Index;
            Size = chunk.Size;

            ModifiedBlockLayers = new IBlockLayerStorage[]
            {
                new SparseBlockStorage(),  // BlockLayer.Main
                new SparseBlockStorage(),  // BlockLayer.Behind
                new SparseBlockStorage()   // BlockLayer.Front
            };
            ModifiedBlockStyleOverrides = new();
            ModifiedInventoriesByLayer = new();
        }
        #endregion

        #region Применить измененя
        public void ApplyChunk(Chunk chunk)
        {
            if (_isApplied)
                throw new InvalidOperationException($"ChunkDiff.ApplyChunk: chunk already applied!");

            _chunk = chunk;
            
            ApplyDiff();
        }
        public void ApplyDiff()
        {
            for (byte i = 0; i < ModifiedBlockLayers.Length; i++)
            {
                BlockLayer layer = (BlockLayer)i;
                IBlockLayerStorage storage = ModifiedBlockLayers[i];
                foreach (KeyValuePair<BlockIndex, Block> kvp in storage)
                {
                    BlockIndex index = kvp.Key;
                    Block block = kvp.Value;
                    _chunk.Blocks.SetSilent(index, block, layer);
                }
            }

            foreach (var stylesOverrides in ModifiedBlockStyleOverrides)
            {
                BlockLayer layer = stylesOverrides.Key;
                var overrides = stylesOverrides.Value;
                foreach (var overrided in overrides)
                {
                    BlockIndex index = overrided.Key;
                    BlockStyles styles = overrided.Value;
                    _chunk.BlockStyles.OverrideBlockStylesSilent(index, styles, layer);
                }
            }

            foreach (var inventoriesByLayer in ModifiedInventoriesByLayer)
            {
                BlockLayer layer = inventoriesByLayer.Key;
                var inventories = inventoriesByLayer.Value;
                foreach (var inventoryKVP in inventories)
                {
                    BlockIndex index = inventoryKVP.Key;
                    BlockInventory inventory = inventoryKVP.Value;
                    _chunk.Inventories.OverrideInventorySilent(index, layer, inventory);
                }
            }

            _isApplied = true;
        }
        #endregion

        #region Подписка на события
        public void SubscribeToChunkEvents()
        {
            _chunk.Events.OnBlockSet += HandleBlockSet;
            _chunk.Events.OnBlockBroken += HandleBlockBroken;
            _chunk.Events.OnBlockStylesCreated += HandleBlockStylesCreated;
            _chunk.Events.OnBlockStylesRemoved += HandleBlockStylesRemoved;
            _chunk.Events.OnBlockInventoryCreated += HandleBlockInventoryCreated;
            _chunk.Events.OnBlockInventoryRemoved += HandleBlockInventoryRemoved;
            _isSubscribedToEvents = true;
        }
        public void UnsubscribeFromChunkEvents()
        {
            _chunk.Events.OnBlockSet -= HandleBlockSet;
            _chunk.Events.OnBlockBroken -= HandleBlockBroken;
            _chunk.Events.OnBlockStylesCreated -= HandleBlockStylesCreated;
            _chunk.Events.OnBlockStylesRemoved -= HandleBlockStylesRemoved;
            _chunk.Events.OnBlockInventoryCreated -= HandleBlockInventoryCreated;
            _chunk.Events.OnBlockInventoryRemoved -= HandleBlockInventoryRemoved;
            _isSubscribedToEvents = false;
        }
        #endregion

        #region Обработка событмй
        private void HandleBlockSet(BlockIndex index, Block block, BlockLayer layer)
        {
            ModifiedBlockLayers[(byte)layer].Set(index, block);
            OnChanged?.Invoke();
        }
        private void HandleBlockBroken(BlockIndex index, Block oldBlock, BlockLayer layer)
        {
            ModifiedBlockLayers[(byte)layer].Set(index, Block.Air);
            OnChanged?.Invoke();
        }

        private void HandleBlockStylesCreated(BlockIndex index, BlockStyles styles, BlockLayer layer)
        {
            (ModifiedBlockStyleOverrides[layer] ??= new())[index] = styles;
            OnChanged?.Invoke();
        }
        private void HandleBlockStylesRemoved(BlockIndex index, BlockStyles styles, BlockLayer layer)
        {
            if (ModifiedBlockStyleOverrides.TryGetValue(layer, out var overrides)) overrides.Remove(index);
            OnChanged?.Invoke();
        }

        private void HandleBlockInventoryCreated(BlockIndex index, BlockInventory inventory, BlockLayer layer)
        {
            (ModifiedInventoriesByLayer[layer] ??= new())[index] = inventory;
            OnChanged?.Invoke();
        }
        private void HandleBlockInventoryRemoved(BlockIndex index, BlockInventory inventory, BlockLayer layer)
        {
            if (ModifiedInventoriesByLayer.TryGetValue(layer, out var overrides)) overrides.Remove(index);
            OnChanged?.Invoke();
        }
        #endregion
    }
}