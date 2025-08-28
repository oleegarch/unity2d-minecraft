using System;
using System.Collections.Generic;
using World.Blocks;
using World.Inventories;

namespace World.Chunks.BlocksStorage
{
    // Интерфейс для логики, связанной с инвентарём блоков
    public interface IChunkBlockInventories : IDisposable
    {
        public void OverrideInventory(BlockIndex index, BlockLayer layer, Inventory inventory);
        public bool TryGetInventory(BlockIndex index, BlockLayer layer, out Inventory inventory);
    }
    public class ChunkBlockInventories : IChunkBlockInventories
    {
        private readonly Dictionary<BlockLayer, Dictionary<BlockIndex, Inventory>> _inventoriesByLayer = new();
        private readonly IChunkBlockModifier _blocks;

        public ChunkBlockInventories(IChunkBlockModifier blocks)
        {
            _blocks = blocks;
            _blocks.Events.OnBlockBroken += HandleBlockBroken;
        }

        public void OverrideInventory(BlockIndex index, BlockLayer layer, Inventory inventory)
        {
            if (!_inventoriesByLayer.TryGetValue(layer, out var _inventories))
                _inventories = _inventoriesByLayer[layer] = new();

            _inventories[index] = inventory;
        }
        public bool TryGetInventory(BlockIndex index, BlockLayer layer, out Inventory inventory)
        {
            if (!_inventoriesByLayer.TryGetValue(layer, out var _inventories) || !_inventories.ContainsKey(index))
            {
                inventory = null;
                return false;
            }

            inventory = _inventories[index];
            return true;
        }

        private void HandleBlockBroken(BlockIndex index, Block block, BlockLayer layer)
        {
            if (TryGetInventory(index, layer, out Inventory inventory))
                _blocks.Events.InvokeBlockInventoryDropped(index, inventory, layer);
        }

        public void Dispose()
        {
            _blocks.Events.OnBlockBroken -= HandleBlockBroken;
        }
    }
}