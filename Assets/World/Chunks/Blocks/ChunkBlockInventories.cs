using System;
using System.Collections.Generic;
using World.Blocks;
using World.Inventories;

namespace World.Chunks.Blocks
{
    // Интерфейс для логики, связанной с инвентарём блоков
    public interface IChunkBlockInventories : IDisposable
    {
        public void OverrideInventory(BlockIndex index, BlockLayer layer, BlockInventory inventory);
        public bool TryGetInventory(BlockIndex index, BlockLayer layer, out BlockInventory inventory);
    }
    public class ChunkBlockInventories : IChunkBlockInventories
    {
        private readonly Dictionary<BlockLayer, Dictionary<BlockIndex, BlockInventory>> _inventoriesByLayer = new();
        private readonly ChunkBlockEvents _events;

        public ChunkBlockInventories(ChunkBlockEvents events)
        {
            _events = events;
            _events.OnBlockBroken += HandleBlockBroken;
        }

        public void OverrideInventory(BlockIndex index, BlockLayer layer, BlockInventory inventory)
        {
            if (!_inventoriesByLayer.TryGetValue(layer, out var inventories))
                inventories = _inventoriesByLayer[layer] = new();

            inventories[index] = inventory;
            _events.InvokeBlockInventoryCreated(index, inventory, layer);
        }
        public bool RemoveInventory(BlockIndex index, BlockLayer layer)
        {
            if (!_inventoriesByLayer.TryGetValue(layer, out var inventories) || !inventories.ContainsKey(index))
                return false;

            _events.InvokeBlockInventoryRemoved(index, inventories[index], layer);
            inventories.Remove(index);

            return true;
        }
        public bool TryGetInventory(BlockIndex index, BlockLayer layer, out BlockInventory inventory)
        {
            if (_inventoriesByLayer.TryGetValue(layer, out var inventories) && inventories.TryGetValue(index, out inventory))
                return true;
            inventory = null;
            return false;
        }

        private void HandleBlockBroken(BlockIndex index, Block block, BlockLayer layer)
        {
            RemoveInventory(index, layer);
        }

        public void Dispose()
        {
            _events.OnBlockBroken -= HandleBlockBroken;
        }
    }
}