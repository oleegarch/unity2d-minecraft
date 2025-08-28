using System;
using System.Collections.Generic;
using World.Blocks;
using World.Inventories;

namespace World.Chunks.BlocksStorage
{
    // Интерфейс для логики, связанной с инвентарём блоков
    public interface IChunkBlockInventories : IDisposable
    {
        public void OverrideInventory(BlockIndex index, BlockLayer layer, IInventory inventory);
        public IInventory GetInventory(BlockIndex index, BlockLayer layer);
    }
    public class ChunkBlockInventories : IChunkBlockInventories
    {
        private readonly Dictionary<BlockLayer, Dictionary<BlockIndex, IInventory>> _inventoriesByLayer = new();
        private readonly IChunkBlockModifier _blocks;

        public ChunkBlockInventories(IChunkBlockModifier blocks)
        {
            _blocks = blocks;
            _blocks.Events.OnBlockBroken += HandleBlockBroken;
        }

        public void OverrideInventory(BlockIndex index, BlockLayer layer, IInventory inventory)
        {
            if (!_inventoriesByLayer.TryGetValue(layer, out var _inventories))
                _inventories = _inventoriesByLayer[layer] = new();

            _inventories[index] = inventory;
        }
        public IInventory GetInventory(BlockIndex index, BlockLayer layer)
        {
            if (!_inventoriesByLayer.TryGetValue(layer, out var _inventories) || !_inventories.ContainsKey(index))
                return null;

            return _inventories[index];
        }

        private void HandleBlockBroken(BlockIndex index, Block block, BlockLayer layer)
        {
            IInventory inventory = GetInventory(index, layer);
            if (inventory != null)
                _blocks.Events.InvokeBlockInventoryDropped(index, inventory, layer);
        }

        public void Dispose()
        {
            _blocks.Events.OnBlockBroken -= HandleBlockBroken;
        }
    }
}