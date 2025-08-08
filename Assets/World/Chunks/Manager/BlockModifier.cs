using World.Blocks;
using World.Chunks.BlocksStorage;

namespace World.Chunks
{
    public interface IBlockModifier
    {
        public Block Get(WorldPosition worldPosition, BlockLayer blockLayer = BlockLayer.Main);
        public Block GetBreakable(WorldPosition worldPosition, out BlockLayer blockLayer);
        public bool Set(WorldPosition worldPosition, Block block, BlockLayer layer);
        public bool Break(WorldPosition worldPosition, BlockLayer layer);
        public bool BreakVisible(WorldPosition worldPosition);
    }
    public class BlockModifier : IBlockModifier
    {
        private readonly IChunksStorage _chunksStorage;

        public BlockEvents Events { get; private set; }

        public BlockModifier(IChunksStorage storage)
        {
            _chunksStorage = storage;
            Events = new BlockEvents();
        }

        public Block Get(WorldPosition worldPosition, BlockLayer blockLayer = BlockLayer.Main)
        {
            if (_chunksStorage.TryGetChunk(worldPosition, out Chunk chunk))
            {
                BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
                Block mainBlock = chunk.GetBlock(blockIndex, blockLayer);

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
                Block mainBlock = chunk.GetBlock(blockIndex, blockLayer);

                if (mainBlock.IsAir())
                {
                    blockLayer = BlockLayer.Behind;
                    return chunk.GetBlock(blockIndex, blockLayer);
                }

                return mainBlock;
            }

            return Block.Air;
        }

        public bool Set(WorldPosition worldPosition, Block block, BlockLayer layer)
        {
            if (!_chunksStorage.TryGetChunk(worldPosition, out var chunk))
                return false;

            BlockIndex blockIndex = worldPosition.ToBlockIndex(chunk.Size);
            if (!chunk.TrySetBlock(blockIndex, block, layer))
                return false;

            if (_chunksStorage.TryGetRenderer(worldPosition, out var renderer))
            {
                renderer.Mesh.DrawBlock(blockIndex);
                renderer.Collider.AddSquare(blockIndex);
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
            if (!chunk.TryUnsetBlock(blockIndex, layer))
                return false;

            if (_chunksStorage.TryGetRenderer(worldPosition, out var renderer))
            {
                Block block = chunk.GetBlock(blockIndex, layer);
                renderer.Mesh.EraseBlock(blockIndex);
                renderer.Collider.RemoveSquare(blockIndex);
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
            Block block = chunk.GetBlock(blockIndex, layer);

            if (block.IsAir())
            {
                layer = BlockLayer.Behind;
                block = chunk.GetBlock(blockIndex, layer);
            }

            if (!chunk.TryUnsetBlock(blockIndex, layer))
                return false;

            if (_chunksStorage.TryGetRenderer(worldPosition, out var renderer))
            {
                renderer.Mesh.EraseBlock(blockIndex);
                renderer.Collider.RemoveSquare(blockIndex);
                Events.InvokeBlockBroken(worldPosition, block, layer);
                return true;
            }

            return false;
        }
    }
}