using System;
using System.Collections.Generic;
using R3;
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
        private readonly List<IDisposable> _subscriptions = new();
        private bool _isSubscribedToEvents = false;

        public void SubscribeToChunkEvents()
        {
            if (_isSubscribedToEvents) return;

            _subscriptions.Add(_chunk.Events.BlockSet.Subscribe(HandleBlockSet));
            _subscriptions.Add(_chunk.Events.BlockBroken.Subscribe(HandleBlockBroken));
            _subscriptions.Add(_chunk.Events.BlockStylesCreated.Subscribe(HandleBlockStylesCreated));
            _subscriptions.Add(_chunk.Events.BlockStylesRemoved.Subscribe(HandleBlockStylesRemoved));
            _subscriptions.Add(_chunk.Events.BlockInventoryCreated.Subscribe(HandleBlockInventoryCreated));
            _subscriptions.Add(_chunk.Events.BlockInventoryRemoved.Subscribe(HandleBlockInventoryRemoved));

            _isSubscribedToEvents = true;
        }

        public void UnsubscribeFromChunkEvents()
        {
            if (!_isSubscribedToEvents) return;

            foreach (var sub in _subscriptions)
                sub.Dispose();
            _subscriptions.Clear();

            _isSubscribedToEvents = false;
        }
        #endregion

        #region Обработка событмй
        private void HandleBlockSet(BlockEvent e)
        {
            Data.ModifiedBlockLayers[(byte)e.Layer].Set(e.Index, e.Block);
            OnDataChanged?.Invoke();
        }
        private void HandleBlockBroken(BlockEvent e)
        {
            Data.ModifiedBlockLayers[(byte)e.Layer].Set(e.Index, Block.Air);
            OnDataChanged?.Invoke();
        }
        private void HandleBlockStylesCreated(BlockStylesEvent e)
        {
            Data.ModifiedBlockStyleOverrides.GetOrCreate(e.Layer).Add(e.Index, e.Styles);
            OnDataChanged?.Invoke();
        }
        private void HandleBlockStylesRemoved(BlockStylesEvent e)
        {
            if (Data.ModifiedBlockStyleOverrides.TryGetValue(e.Layer, out var overrides))
                overrides.Remove(e.Index);
            OnDataChanged?.Invoke();
        }
        private void HandleBlockInventoryCreated(BlockInventoryEvent e)
        {
            Data.ModifiedInventoriesByLayer.GetOrCreate(e.Layer).Add(e.Index, e.Inventory);
            OnDataChanged?.Invoke();
        }
        private void HandleBlockInventoryRemoved(BlockInventoryEvent e)
        {
            if (Data.ModifiedInventoriesByLayer.TryGetValue(e.Layer, out var overrides)) overrides.Remove(e.Index);
            OnDataChanged?.Invoke();
        }
        #endregion
    }
}