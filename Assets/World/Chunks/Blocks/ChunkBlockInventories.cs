using System;
using System.Collections.Generic;
using R3;
using World.Blocks;
using World.Inventories;

namespace World.Chunks.Blocks
{
    #region Интерфейс
    // Интерфейс для логики, связанной с инвентарём блоков
    public interface IChunkBlockInventories : IDisposable
    {
        /// <summary>Попытать получить инвентарь</summary>
        public bool TryGetInventory(BlockIndex index, BlockLayer layer, out BlockInventory inventory);
        /// <summary>Перезаписать новый инвентарь с событиями (если есть предыдущий — он будет удалён)</summary>
        public void OverrideInventory(BlockIndex index, BlockLayer layer, BlockInventory inventory);
        /// <summary>Перезаписать новый инвентарь без событий (если есть предыдущий — он будет удалён)</summary>
        public void OverrideInventorySilent(BlockIndex index, BlockLayer layer, BlockInventory inventory);
        /// <summary>Удалить инвентарь с событием</summary>
        public bool RemoveInventory(BlockIndex index, BlockLayer layer);
        /// <summary>Удалить инвентарь без события</summary>
        public bool RemoveInventorySilent(BlockIndex index, BlockLayer layer);
    }
    #endregion
    public class ChunkBlockInventories : IChunkBlockInventories
    {
        #region Конструктор
        private readonly Dictionary<BlockLayer, Dictionary<BlockIndex, BlockInventory>> _inventoriesByLayer = new();
        private readonly ChunkBlockEvents _events;
        private readonly IDisposable _blockBrokenSub;

        public ChunkBlockInventories(ChunkBlockEvents events)
        {
            _events = events;

            _blockBrokenSub = _events.BlockBroken.Subscribe(be =>
            {
                RemoveInventory(be.Index, be.Layer);
            });
        }
        #endregion

        #region Публичные методы
        public bool TryGetInventory(BlockIndex index, BlockLayer layer, out BlockInventory inventory)
        {
            if (_inventoriesByLayer.TryGetValue(layer, out var inventories) && inventories.TryGetValue(index, out inventory))
                return true;
            inventory = null;
            return false;
        }
        public void OverrideInventorySilent(BlockIndex index, BlockLayer layer, BlockInventory inventory)
        {
            _OverrideInventory(index, layer, inventory);
        }
        public void OverrideInventory(BlockIndex index, BlockLayer layer, BlockInventory inventory)
        {
            if (_RemoveInventory(index, layer, out BlockInventory removedInventory))
                _events.InvokeBlockInventoryRemoved(index, removedInventory, layer);

            _OverrideInventory(index, layer, inventory);
            _events.InvokeBlockInventoryCreated(index, inventory, layer);
        }
        public bool RemoveInventorySilent(BlockIndex index, BlockLayer layer)
        {
            return _RemoveInventory(index, layer, out BlockInventory removedInventory);
        }
        public bool RemoveInventory(BlockIndex index, BlockLayer layer)
        {
            if (_RemoveInventory(index, layer, out BlockInventory removedInventory))
            {
                _events.InvokeBlockInventoryRemoved(index, removedInventory, layer);
                return true;
            }
            
            return false;
        }
        #endregion

        #region Сама реализация
        private void _OverrideInventory(BlockIndex index, BlockLayer layer, BlockInventory inventory)
        {
            if (!_inventoriesByLayer.TryGetValue(layer, out var inventories))
                inventories = _inventoriesByLayer[layer] = new();

            inventories[index] = inventory;
        }
        private bool _RemoveInventory(BlockIndex index, BlockLayer layer, out BlockInventory removedInventory)
        {
            if (!_inventoriesByLayer.TryGetValue(layer, out var inventories) || !inventories.ContainsKey(index))
            {
                removedInventory = null;
                return false;
            }

            removedInventory = inventories[index];
            inventories.Remove(index);

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