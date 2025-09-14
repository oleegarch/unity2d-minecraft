using System;
using System.Collections.Generic;
using World.Blocks;
using World.Chunks.Blocks;
using World.Inventories;

namespace World.Chunks
{
    public class WorldBlockEvents : IDisposable
    {
        // Подписка на создание и удаление чанка
        private IChunksAccessor _creator;

        public WorldBlockEvents(ChunksCreator creator)
        {
            _creator = creator;
            _creator.OnChunkCreated += SubscribeToChunkEvents;
            _creator.OnChunkBeforeRemove += UnsubscribeFromChunkEvents;
        }
        public void Dispose()
        {
            _creator.OnChunkCreated -= SubscribeToChunkEvents;
            _creator.OnChunkBeforeRemove -= UnsubscribeFromChunkEvents;
        }

        // BLOCKS
        public event Action<WorldPosition, Block, BlockLayer> OnBlockSet;
        public event Action<WorldPosition, Block, BlockLayer> OnBlockSetByPlayer;
        public event Action<WorldPosition, Block, BlockLayer> OnBlockBroken;
        public event Action<WorldPosition, Block, BlockLayer> OnBlockBrokenByPlayer;

        // BLOCK STYLES
        public event Action<WorldPosition, BlockStyles, BlockLayer> OnBlockStylesCreated;
        public event Action<WorldPosition, BlockStyles, BlockLayer> OnBlockStylesRemoved;

        // INVENTORIES
        public event Action<WorldPosition, BlockInventory, BlockLayer> OnBlockInventoryCreated;
        public event Action<WorldPosition, BlockInventory, BlockLayer> OnBlockInventoryRemoved;

        // Храним делегаты, чтобы можно было корректно отписаться потом
        private readonly Dictionary<Chunk, Subscriptions> _subscriptions = new();
        private class Subscriptions
        {
            public Action<BlockIndex, Block, BlockLayer> BlockSet;
            public Action<BlockIndex, Block, BlockLayer> BlockSetByPlayer;
            public Action<BlockIndex, Block, BlockLayer> BlockBroken;
            public Action<BlockIndex, Block, BlockLayer> BlockBrokenByPlayer;
            public Action<BlockIndex, BlockStyles, BlockLayer> BlockStylesCreated;
            public Action<BlockIndex, BlockStyles, BlockLayer> BlockStylesRemoved;
            public Action<BlockIndex, BlockInventory, BlockLayer> BlockInventoryCreated;
            public Action<BlockIndex, BlockInventory, BlockLayer> BlockInventoryRemoved;
        }
        private Action<BlockIndex, T, BlockLayer> Proxy<T>(Chunk chunk, Action<WorldPosition, T, BlockLayer> worldEvent)
        {
            return (blockIndex, arg, layer) =>
            {
                var worldPos = chunk.Index.ToWorldPosition(blockIndex, chunk.Size);
                worldEvent?.Invoke(worldPos, arg, layer);
            };
        }
        private Subscriptions CreateProxySubscriptions(Chunk chunk)
        {
            var subs = new Subscriptions();
            subs.BlockSet = Proxy(chunk, OnBlockSet);
            subs.BlockSetByPlayer = Proxy(chunk, OnBlockSetByPlayer);
            subs.BlockBroken = Proxy(chunk, OnBlockBroken);
            subs.BlockBrokenByPlayer = Proxy(chunk, OnBlockBrokenByPlayer);
            subs.BlockStylesCreated = Proxy(chunk, OnBlockStylesCreated);
            subs.BlockStylesRemoved = Proxy(chunk, OnBlockStylesRemoved);
            subs.BlockInventoryCreated = Proxy(chunk, OnBlockInventoryCreated);
            subs.BlockInventoryRemoved = Proxy(chunk, OnBlockInventoryRemoved);
            return subs;
        }

        public void SubscribeToChunkEvents(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException(nameof(chunk));
            if (chunk.Events == null) throw new ArgumentException("chunk.Events is null", nameof(chunk));
            if (_subscriptions.ContainsKey(chunk)) return; // уже подписаны

            var subs = CreateProxySubscriptions(chunk);

            // Подписываемся на события чанка
            chunk.Events.OnBlockSet += subs.BlockSet;
            chunk.Events.OnBlockSetByPlayer += subs.BlockSetByPlayer;
            chunk.Events.OnBlockBroken += subs.BlockBroken;
            chunk.Events.OnBlockBrokenByPlayer += subs.BlockBrokenByPlayer;
            chunk.Events.OnBlockStylesCreated += subs.BlockStylesCreated;
            chunk.Events.OnBlockStylesRemoved += subs.BlockStylesRemoved;
            chunk.Events.OnBlockInventoryCreated += subs.BlockInventoryCreated;
            chunk.Events.OnBlockInventoryRemoved += subs.BlockInventoryRemoved;

            _subscriptions.Add(chunk, subs);
        }

        public void UnsubscribeFromChunkEvents(Chunk chunk)
        {
            if (chunk == null) throw new ArgumentNullException(nameof(chunk));
            if (!_subscriptions.TryGetValue(chunk, out var subs)) return;
            if (chunk.Events != null)
            {
                chunk.Events.OnBlockSet -= subs.BlockSet;
                chunk.Events.OnBlockSetByPlayer -= subs.BlockSetByPlayer;
                chunk.Events.OnBlockBroken -= subs.BlockBroken;
                chunk.Events.OnBlockBrokenByPlayer -= subs.BlockBrokenByPlayer;
                chunk.Events.OnBlockStylesCreated -= subs.BlockStylesCreated;
                chunk.Events.OnBlockStylesRemoved -= subs.BlockStylesRemoved;
                chunk.Events.OnBlockInventoryCreated -= subs.BlockInventoryCreated;
                chunk.Events.OnBlockInventoryRemoved -= subs.BlockInventoryRemoved;
            }

            _subscriptions.Remove(chunk);
        }
    }
}