using System;
using System.Collections.Generic;
using World.Blocks;
using World.Chunks.BlocksStorage;
using World.Inventories;

namespace World.Chunks
{
    public class WorldBlockEvents
    {
        public event Action<WorldPosition, Block, BlockLayer> OnBlockSet;
        public event Action<WorldPosition, Block, BlockLayer> OnBlockBroken;
        public event Action<WorldPosition, Block, BlockLayer> OnBlockSetByPlayer;
        public event Action<WorldPosition, Block, BlockLayer> OnBlockBrokenByPlayer;
        public event Action<WorldPosition, IInventory, BlockLayer> OnBlockInventoryDropped;

        // Храним делегаты, чтобы можно было корректно отписаться потом
        private class Subscriptions
        {
            public Action<BlockIndex, Block, BlockLayer> BlockSet;
            public Action<BlockIndex, Block, BlockLayer> BlockBroken;
            public Action<BlockIndex, Block, BlockLayer> BlockSetByPlayer;
            public Action<BlockIndex, Block, BlockLayer> BlockBrokenByPlayer;
            public Action<BlockIndex, IInventory, BlockLayer> BlockInventoryDropped;
        }

        private readonly Dictionary<Chunk, Subscriptions> _subscriptions = new Dictionary<Chunk, Subscriptions>();

        public void SubscribeToChunkEvents(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException(nameof(chunk));
            if (chunk.Blocks?.Events == null) throw new ArgumentException("chunk.Blocks.Events is null", nameof(chunk));
            if (_subscriptions.ContainsKey(chunk)) return; // уже подписаны

            var subs = new Subscriptions();

            subs.BlockSet = (blockIndex, block, layer) =>
            {
                var worldPos = chunk.Index.ToWorldPosition(blockIndex, chunk.Size);
                OnBlockSet?.Invoke(worldPos, block, layer);
            };

            subs.BlockBroken = (blockIndex, block, layer) =>
            {
                var worldPos = chunk.Index.ToWorldPosition(blockIndex, chunk.Size);
                OnBlockBroken?.Invoke(worldPos, block, layer);
            };

            subs.BlockSetByPlayer = (blockIndex, block, layer) =>
            {
                var worldPos = chunk.Index.ToWorldPosition(blockIndex, chunk.Size);
                OnBlockSetByPlayer?.Invoke(worldPos, block, layer);
            };

            subs.BlockBrokenByPlayer = (blockIndex, block, layer) =>
            {
                var worldPos = chunk.Index.ToWorldPosition(blockIndex, chunk.Size);
                OnBlockBrokenByPlayer?.Invoke(worldPos, block, layer);
            };

            subs.BlockInventoryDropped = (blockIndex, inventory, layer) =>
            {
                var worldPos = chunk.Index.ToWorldPosition(blockIndex, chunk.Size);
                OnBlockInventoryDropped?.Invoke(worldPos, inventory, layer);
            };

            // Подписываемся на события чанка
            chunk.Blocks.Events.OnBlockSet += subs.BlockSet;
            chunk.Blocks.Events.OnBlockBroken += subs.BlockBroken;
            chunk.Blocks.Events.OnBlockSetByPlayer += subs.BlockSetByPlayer;
            chunk.Blocks.Events.OnBlockBrokenByPlayer += subs.BlockBrokenByPlayer;
            chunk.Blocks.Events.OnBlockInventoryDropped += subs.BlockInventoryDropped;

            _subscriptions.Add(chunk, subs);
        }

        public void UnsubscribeFromChunkEvents(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException(nameof(chunk));
            if (!_subscriptions.TryGetValue(chunk, out var subs)) return; // не подписаны — ничего делать не нужно
            if (chunk.Blocks?.Events != null)
            {
                chunk.Blocks.Events.OnBlockSet -= subs.BlockSet;
                chunk.Blocks.Events.OnBlockBroken -= subs.BlockBroken;
                chunk.Blocks.Events.OnBlockSetByPlayer -= subs.BlockSetByPlayer;
                chunk.Blocks.Events.OnBlockBrokenByPlayer -= subs.BlockBrokenByPlayer;
                chunk.Blocks.Events.OnBlockInventoryDropped -= subs.BlockInventoryDropped;
            }

            _subscriptions.Remove(chunk);
        }
    }
}