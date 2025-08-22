using World.Blocks;
using World.Chunks.BlocksStorage;

namespace World.Chunks
{
    public interface IChunksBlockModifier
    {
        public ChunksBlockEvents Events { get; }
        public Block Get(WorldPosition worldPosition, BlockLayer blockLayer = BlockLayer.Main);
        public Block GetBreakable(WorldPosition worldPosition, out BlockLayer blockLayer);
        public BlockStyles GetBlockStyles(WorldPosition worldPosition, BlockLayer layer);
        public bool Set(WorldPosition worldPosition, Block block, BlockLayer layer, BlockStyles styles);
        public bool Break(WorldPosition worldPosition, BlockLayer layer);
        public bool BreakVisible(WorldPosition worldPosition);
    }
    public class ChunksBlockModifier : IChunksBlockModifier
    {
        private readonly IChunksStorage _chunksStorage;

        public ChunksBlockEvents Events { get; private set; }

        public ChunksBlockModifier(IChunksStorage storage)
        {
            _chunksStorage = storage;
            Events = new ChunksBlockEvents();
        }

        public Block Get(WorldPosition worldPosition, BlockLayer blockLayer = BlockLayer.Main)
        {
            if (_chunksStorage.TryGetChunk(worldPosition, out Chunk chunk))
            {
                BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
                Block mainBlock = chunk.Blocks.Get(blockIndex, blockLayer);

                return mainBlock;
            }

            return Block.Air;
        }
        public Block GetBreakable(WorldPosition worldPosition, out BlockLayer blockLayer)
        {
            blockLayer = BlockLayer.Main;

            if (_chunksStorage.TryGetChunk(worldPosition, out Chunk chunk))
            {
                BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
                Block mainBlock = chunk.Blocks.Get(blockIndex, blockLayer);

                if (mainBlock.IsAir)
                {
                    blockLayer = BlockLayer.Behind;
                    return chunk.Blocks.Get(blockIndex, blockLayer);
                }

                return mainBlock;
            }

            return Block.Air;
        }
        public BlockStyles GetBlockStyles(WorldPosition worldPosition, BlockLayer layer)
        {
            if (_chunksStorage.TryGetChunk(worldPosition, out Chunk chunk))
            {
                BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
                return chunk.Render.GetBlockStyles(blockIndex, layer);
            }
            return BlockStyles.ByLayer[(int)layer];
        }

        public bool Set(WorldPosition worldPosition, Block block, BlockLayer layer, BlockStyles styles)
        {
            if (!_chunksStorage.TryGetChunk(worldPosition, out var chunk))
                return false;

            BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
            if (chunk.Render.TrySet(blockIndex, block, styles, layer))
            {
                Events.InvokeBlockSet(worldPosition, block, layer);
                return true;
            }

            return false;
        }

        public bool Break(WorldPosition worldPosition, BlockLayer layer)
        {
            if (!_chunksStorage.TryGetChunk(worldPosition, out var chunk))
                return false;

            BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
            Block block = chunk.Blocks.Get(blockIndex, layer);
            if (chunk.Blocks.TryUnset(blockIndex, layer))
            {
                Events.InvokeBlockBroken(worldPosition, block, layer);
                return true;
            }

            return false;
        }

        public bool BreakVisible(WorldPosition worldPosition)
        {
            if (!_chunksStorage.TryGetChunk(worldPosition, out var chunk))
                return false;

            BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
            BlockLayer layer = BlockLayer.Main;
            Block block = chunk.Blocks.Get(blockIndex, layer);

            if (block.IsAir)
            {
                layer = BlockLayer.Behind;
                block = chunk.Blocks.Get(blockIndex, layer);
            }

            if (chunk.Blocks.TryUnset(blockIndex, layer))
            {
                Events.InvokeBlockBroken(worldPosition, block, layer);
                return true;
            }

            return false;
        }
    }
}