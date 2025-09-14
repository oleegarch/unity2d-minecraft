using System;
using System.Collections.Generic;
using World.Blocks;
using World.Chunks.Blocks;
using World.Chunks.Blocks.Storages;
using World.Inventories;

namespace World.Chunks.Storage
{
    #region Данные хранилища
    /// <summary>
    /// Класс хранящий изменения произошедшие в Chunk.
    /// </summary>
    [Serializable]
    public class ChunkDiffData
    {
        public readonly ChunkIndex Index;
        public readonly byte Size;
        public readonly IBlockLayerStorage[] ModifiedBlockLayers;
        public readonly Dictionary<BlockLayer, Dictionary<BlockIndex, BlockStyles>> ModifiedBlockStyleOverrides;
        public readonly Dictionary<BlockLayer, Dictionary<BlockIndex, BlockInventory>> ModifiedInventoriesByLayer;

        public ChunkDiffData(ChunkIndex index, byte size)
        {
            Index = index;
            Size = size;
            ModifiedBlockLayers = new IBlockLayerStorage[]
            {
                new SparseBlockStorage(),  // BlockLayer.Main
                new SparseBlockStorage(),  // BlockLayer.Behind
                new SparseBlockStorage()   // BlockLayer.Front
            };
            ModifiedBlockStyleOverrides = new();
            ModifiedInventoriesByLayer = new();
        }
    }
    #endregion

    /// <summary>
    /// Класс отвечающий за слушание изменений в Chunk и запись этих изменений в Data(ChunkDiffData).
    /// </summary>
    public class ChunkDiff
    {
        #region Конструктор и поля
        private Chunk _chunk;
        private bool _isApplied = false;
        private bool _isSubscribedToEvents = false;
        public bool IsLinked => _chunk != null;
        public bool IsApplied => _isApplied;
        public bool IsSubscribedToEvents => _isSubscribedToEvents;

        public ChunkDiffData Data;
        public event Action OnDataChanged;

        public ChunkDiff(ChunkDiffData data)
        {
            Data = data;
        }
        #endregion

        #region Привязать чанк
        public void LinkChunk(Chunk chunk)
        {
            if (IsLinked) throw new InvalidOperationException($"ChunkDiff.ApplyChunk: chunk already linked!");

            _chunk = chunk;
        }
        public void UnlinkChunk()
        {
            if (!IsLinked) throw new InvalidOperationException($"ChunkDiff.UnlinkChunk: chunk not linked!");

            _chunk = null;
            _isApplied = false;
        }
        #endregion

        #region Применить измененя
        public void ApplyDiff()
        {
            for (byte i = 0; i < Data.ModifiedBlockLayers.Length; i++)
            {
                BlockLayer layer = (BlockLayer)i;
                IBlockLayerStorage storage = Data.ModifiedBlockLayers[i];
                foreach (KeyValuePair<BlockIndex, Block> kvp in storage)
                {
                    BlockIndex index = kvp.Key;
                    Block block = kvp.Value;
                    _chunk.Blocks.SetSilent(index, block, layer);
                }
            }

            foreach (var stylesOverrides in Data.ModifiedBlockStyleOverrides)
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

            foreach (var inventoriesByLayer in Data.ModifiedInventoriesByLayer)
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
            if (_isSubscribedToEvents == false)
            {
                _chunk.Events.OnBlockSet += HandleBlockSet;
                _chunk.Events.OnBlockBroken += HandleBlockBroken;
                _chunk.Events.OnBlockStylesCreated += HandleBlockStylesCreated;
                _chunk.Events.OnBlockStylesRemoved += HandleBlockStylesRemoved;
                _chunk.Events.OnBlockInventoryCreated += HandleBlockInventoryCreated;
                _chunk.Events.OnBlockInventoryRemoved += HandleBlockInventoryRemoved;
                _isSubscribedToEvents = true;
            }
        }
        public void UnsubscribeFromChunkEvents()
        {
            if (_isSubscribedToEvents == true)
            {
                _chunk.Events.OnBlockSet -= HandleBlockSet;
                _chunk.Events.OnBlockBroken -= HandleBlockBroken;
                _chunk.Events.OnBlockStylesCreated -= HandleBlockStylesCreated;
                _chunk.Events.OnBlockStylesRemoved -= HandleBlockStylesRemoved;
                _chunk.Events.OnBlockInventoryCreated -= HandleBlockInventoryCreated;
                _chunk.Events.OnBlockInventoryRemoved -= HandleBlockInventoryRemoved;
                _isSubscribedToEvents = false;
            }
        }
        #endregion

        #region Обработка событмй
        private void HandleBlockSet(BlockIndex index, Block block, BlockLayer layer)
        {
            Data.ModifiedBlockLayers[(byte)layer].Set(index, block);
            OnDataChanged?.Invoke();
        }
        private void HandleBlockBroken(BlockIndex index, Block oldBlock, BlockLayer layer)
        {
            Data.ModifiedBlockLayers[(byte)layer].Set(index, Block.Air);
            OnDataChanged?.Invoke();
        }

        private void HandleBlockStylesCreated(BlockIndex index, BlockStyles styles, BlockLayer layer)
        {
            Data.ModifiedBlockStyleOverrides.GetOrCreate(layer).Add(index, styles);
            OnDataChanged?.Invoke();
        }
        private void HandleBlockStylesRemoved(BlockIndex index, BlockStyles styles, BlockLayer layer)
        {
            if (Data.ModifiedBlockStyleOverrides.TryGetValue(layer, out var overrides)) overrides.Remove(index);
            OnDataChanged?.Invoke();
        }

        private void HandleBlockInventoryCreated(BlockIndex index, BlockInventory inventory, BlockLayer layer)
        {
            Data.ModifiedInventoriesByLayer.GetOrCreate(layer).Add(index, inventory);
            OnDataChanged?.Invoke();
        }
        private void HandleBlockInventoryRemoved(BlockIndex index, BlockInventory inventory, BlockLayer layer)
        {
            if (Data.ModifiedInventoriesByLayer.TryGetValue(layer, out var overrides)) overrides.Remove(index);
            OnDataChanged?.Invoke();
        }
        #endregion
    }
}