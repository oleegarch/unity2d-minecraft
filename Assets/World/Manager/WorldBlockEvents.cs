using System;
using System.Collections.Generic;
using R3;
using World.Blocks;
using World.Chunks.Blocks;
using World.Inventories;

namespace World.Chunks
{
    public readonly struct WorldBlockEvent
    {
        public readonly WorldPosition Position;
        public readonly Block Block;
        public readonly BlockLayer Layer;
        public WorldBlockEvent(WorldPosition p, Block b, BlockLayer l) { Position = p; Block = b; Layer = l; }
    }

    public readonly struct WorldBlockStylesEvent
    {
        public readonly WorldPosition Position;
        public readonly BlockStyles Styles;
        public readonly BlockLayer Layer;
        public WorldBlockStylesEvent(WorldPosition p, BlockStyles s, BlockLayer l) { Position = p; Styles = s; Layer = l; }
    }

    public readonly struct WorldBlockInventoryEvent
    {
        public readonly WorldPosition Position;
        public readonly BlockInventory Inventory;
        public readonly BlockLayer Layer;
        public WorldBlockInventoryEvent(WorldPosition p, BlockInventory i, BlockLayer l) { Position = p; Inventory = i; Layer = l; }
    }

    public class WorldBlockEvents : IDisposable
    {
        private readonly IChunksAccessor _creator;
        private readonly Dictionary<Chunk, CompositeDisposable> _subscriptions = new();
        private readonly CompositeDisposable _disposables = new();

        public readonly Subject<WorldBlockEvent> BlockSet = new();
        public readonly Subject<WorldBlockEvent> BlockSetByPlayer = new();
        public readonly Subject<WorldBlockEvent> BlockBroken = new();
        public readonly Subject<WorldBlockEvent> BlockBrokenByPlayer = new();

        public readonly Subject<WorldBlockStylesEvent> BlockStylesCreated = new();
        public readonly Subject<WorldBlockStylesEvent> BlockStylesRemoved = new();

        public readonly Subject<WorldBlockInventoryEvent> BlockInventoryCreated = new();
        public readonly Subject<WorldBlockInventoryEvent> BlockInventoryRemoved = new();

        public WorldBlockEvents(IChunksAccessor creator)
        {
            _creator = creator;
            _disposables.Add(_creator.OnChunkCreated.Subscribe(SubscribeToChunk));
            _disposables.Add(_creator.OnChunkBeforeRemove.Subscribe(UnsubscribeFromChunk));
        }

        public void Dispose()
        {
            foreach (var composite in _subscriptions.Values) composite.Dispose();
            _subscriptions.Clear();
            _disposables.Dispose();

            BlockSet.Dispose(); BlockSetByPlayer.Dispose();
            BlockBroken.Dispose(); BlockBrokenByPlayer.Dispose();
            BlockStylesCreated.Dispose(); BlockStylesRemoved.Dispose();
            BlockInventoryCreated.Dispose(); BlockInventoryRemoved.Dispose();
        }

        private void SubscribeToChunk(Chunk chunk)
        {
            if (chunk == null || chunk.Events == null || _subscriptions.ContainsKey(chunk)) return;

            var subs = new IDisposable[]
            {
                chunk.Events.BlockSet.Subscribe(be => {
                    var pos = chunk.Index.ToWorldPosition(be.Index, chunk.Size);
                    BlockSet.OnNext(new WorldBlockEvent(pos, be.Block, be.Layer));
                }),
                chunk.Events.BlockSetByPlayer.Subscribe(be => {
                    var pos = chunk.Index.ToWorldPosition(be.Index, chunk.Size);
                    BlockSetByPlayer.OnNext(new WorldBlockEvent(pos, be.Block, be.Layer));
                }),
                chunk.Events.BlockBroken.Subscribe(be => {
                    var pos = chunk.Index.ToWorldPosition(be.Index, chunk.Size);
                    BlockBroken.OnNext(new WorldBlockEvent(pos, be.Block, be.Layer));
                }),
                chunk.Events.BlockBrokenByPlayer.Subscribe(be => {
                    var pos = chunk.Index.ToWorldPosition(be.Index, chunk.Size);
                    BlockBrokenByPlayer.OnNext(new WorldBlockEvent(pos, be.Block, be.Layer));
                }),
                chunk.Events.BlockStylesCreated.Subscribe(se => {
                    var pos = chunk.Index.ToWorldPosition(se.Index, chunk.Size);
                    BlockStylesCreated.OnNext(new WorldBlockStylesEvent(pos, se.Styles, se.Layer));
                }),
                chunk.Events.BlockStylesRemoved.Subscribe(se => {
                    var pos = chunk.Index.ToWorldPosition(se.Index, chunk.Size);
                    BlockStylesRemoved.OnNext(new WorldBlockStylesEvent(pos, se.Styles, se.Layer));
                }),
                chunk.Events.BlockInventoryCreated.Subscribe(ie => {
                    var pos = chunk.Index.ToWorldPosition(ie.Index, chunk.Size);
                    BlockInventoryCreated.OnNext(new WorldBlockInventoryEvent(pos, ie.Inventory, ie.Layer));
                }),
                chunk.Events.BlockInventoryRemoved.Subscribe(ie => {
                    var pos = chunk.Index.ToWorldPosition(ie.Index, chunk.Size);
                    BlockInventoryRemoved.OnNext(new WorldBlockInventoryEvent(pos, ie.Inventory, ie.Layer));
                })
            };

            _subscriptions[chunk] = new CompositeDisposable(subs);
        }

        private void UnsubscribeFromChunk(Chunk chunk)
        {
            if (!_subscriptions.TryGetValue(chunk, out var composite)) return;
            composite.Dispose();
            _subscriptions.Remove(chunk);
        }
    }
}