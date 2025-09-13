using World.Blocks;
using World.Chunks.Blocks;
using World.Chunks.Generator;
using World.Inventories;

namespace World.Chunks
{
    public enum BlockUpdateSource
    {
        Player,
        System
    }
    public interface IWorldBlockModifier
    {
        public Block Get(WorldPosition worldPosition, BlockLayer blockLayer = BlockLayer.Main);
        public Block GetSimilar(WorldPosition worldPosition, BlockLayer blockLayer, out BlockLayer currentLayer);
        public Block GetBreakable(WorldPosition worldPosition, out BlockLayer blockLayer);
        public bool TryGetInventory(WorldPosition worldPosition, out BlockInventory inventory);
        public BlockStyles GetBlockStyles(WorldPosition worldPosition, BlockLayer layer);
        public bool Set(WorldPosition worldPosition, Block block, BlockLayer layer, BlockStyles styles, BlockUpdateSource source = BlockUpdateSource.Player);
        public bool Break(WorldPosition worldPosition, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player);
        public bool BreakVisible(WorldPosition worldPosition, BlockUpdateSource source = BlockUpdateSource.Player);
    }
    public class WorldBlockModifier : IWorldBlockModifier
    {
        private readonly IWorldChunksAccessor _storage;
        private readonly IWorldGenerator _generator;

        public WorldBlockModifier(IWorldChunksAccessor storage, IWorldGenerator generator)
        {
            _storage = storage;
            _generator = generator;
        }

        public Block Get(WorldPosition worldPosition, BlockLayer blockLayer = BlockLayer.Main)
        {
            if (_storage.TryGetChunk(worldPosition, out Chunk chunk))
            {
                BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
                Block mainBlock = chunk.Blocks.Get(blockIndex, blockLayer);

                return mainBlock;
            }

            return Block.Air;
        }
        public Block GetSimilar(WorldPosition worldPosition, BlockLayer blockLayer, out BlockLayer currentLayer)
        {
            currentLayer = blockLayer;

            Block simple = Get(worldPosition, blockLayer);

            if (simple.IsAir && blockLayer == BlockLayer.Main && GetBlockStyles(worldPosition, BlockLayer.Behind).IsBehind == false)
            {
                currentLayer = BlockLayer.Behind;
                return Get(worldPosition, currentLayer);
            }

            return simple;
        }
        public Block GetBreakable(WorldPosition worldPosition, out BlockLayer blockLayer)
        {
            blockLayer = BlockLayer.Main;

            if (_storage.TryGetChunk(worldPosition, out Chunk chunk))
            {
                BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
                Block mainBlock = chunk.Blocks.Get(blockIndex, blockLayer);

                if (mainBlock.IsAir)
                {
                    blockLayer = BlockLayer.Behind;

                    if (_generator.Rules.CanBreakBehindBlock(worldPosition))
                        return chunk.Blocks.Get(blockIndex, blockLayer);

                    return Block.Air;
                }

                return mainBlock;
            }

            return Block.Air;
        }
        public bool TryGetInventory(WorldPosition worldPosition, out BlockInventory inventory)
        {
            inventory = null;

            if (_storage.TryGetChunk(worldPosition, out Chunk chunk))
            {
                BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
                BlockLayer currentLayer = BlockLayer.Main;
                Block currentBlock = chunk.Blocks.Get(blockIndex, currentLayer);

                if (currentBlock.IsAir)
                {
                    currentLayer = BlockLayer.Behind;
                    currentBlock = chunk.Blocks.Get(blockIndex, currentLayer);
                }

                if (currentBlock.IsAir)
                    return false;

                BlockInfo currentBlockInfo = _generator.Config.BlockDatabase.Get(currentBlock.Id);
                if (currentBlockInfo.Inventory.SlotCount > 0)
                {
                    if (!chunk.Inventories.TryGetInventory(blockIndex, currentLayer, out inventory))
                    {
                        inventory = new BlockInventory(currentBlockInfo.Inventory.SlotCount);
                        chunk.Inventories.OverrideInventory(blockIndex, currentLayer, inventory);
                    }
                    return true;
                }
            }

            return false;
        }
        public BlockStyles GetBlockStyles(WorldPosition worldPosition, BlockLayer layer)
        {
            if (_storage.TryGetChunk(worldPosition, out Chunk chunk))
            {
                BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
                return chunk.BlockStyles.GetBlockStyles(blockIndex, layer);
            }
            return BlockStyles.ByLayer[(int)layer];
        }

        public bool Set(WorldPosition worldPosition, Block block, BlockLayer layer, BlockStyles styles, BlockUpdateSource source = BlockUpdateSource.Player)
        {
            if (!_storage.TryGetChunk(worldPosition, out var chunk))
                return false;

            BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
            return chunk.BlockStyles.TrySet(blockIndex, block, styles, layer);
        }

        public bool Break(WorldPosition worldPosition, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player)
        {
            if (!_storage.TryGetChunk(worldPosition, out var chunk))
                return false;

            BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
            return chunk.Blocks.TryUnset(blockIndex, layer, source);
        }

        public bool BreakVisible(WorldPosition worldPosition, BlockUpdateSource source = BlockUpdateSource.Player)
        {
            if (!_storage.TryGetChunk(worldPosition, out var chunk))
                return false;

            BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
            BlockLayer layer = BlockLayer.Main;
            Block block = chunk.Blocks.Get(blockIndex, layer);

            if (block.IsAir)
                layer = BlockLayer.Behind;

            return chunk.Blocks.TryUnset(blockIndex, layer, source);
        }
    }
}