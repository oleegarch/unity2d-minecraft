using World.Blocks;
using World.Chunks.BlocksStorage.Storages;

namespace World.Chunks
{
    // IBlockAccessor — минимальный интерфейс чтения
    public interface IChunkBlockAccessor
    {
        public IBlockLayerStorage GetLayer(BlockLayer layer);
        public Block Get(BlockIndex index, BlockLayer layer);
    }

    // IChunkBlockModifier — чтение + модификация + события
    public interface IChunkBlockModifier : IChunkBlockAccessor
    {
        public ChunkBlockEvents Events { get; }
        public void SetSilent(BlockIndex index, Block block, BlockLayer layer);
        public void Set(BlockIndex index, Block block, BlockLayer layer, BlockBrokeSource source = BlockBrokeSource.Player);
        public bool TrySet(BlockIndex index, Block block, BlockLayer layer, BlockBrokeSource source = BlockBrokeSource.Player);
        public bool TryUnset(BlockIndex index, BlockLayer layer, BlockBrokeSource source = BlockBrokeSource.Player);
    }
    public class ChunkBlockModifier : IChunkBlockModifier
    {
        private readonly IBlockLayerStorage[] _blockLayers;
        public ChunkBlockEvents Events { get; } = new();

        public ChunkBlockModifier(IBlockLayerStorage[] blockLayers) => _blockLayers = blockLayers;

        public IBlockLayerStorage GetLayer(BlockLayer layer) => _blockLayers[(int)layer];

        public Block Get(BlockIndex index, BlockLayer layer) => _blockLayers[(int)layer].Get(index);

        public void SetSilent(BlockIndex index, Block block, BlockLayer layer)
        {
            _blockLayers[(int)layer].Set(index, block);
        }
        public void Set(BlockIndex index, Block block, BlockLayer layer, BlockBrokeSource source = BlockBrokeSource.Player)
        {
            _blockLayers[(int)layer].Set(index, block);

            if (block.IsAir)
                Events.InvokeBlockBroken(index, block, layer, source);
            else
                Events.InvokeBlockSet(index, block, layer, source);
        }

        public bool TrySet(BlockIndex index, Block block, BlockLayer layer, BlockBrokeSource source = BlockBrokeSource.Player)
        {
            if (Get(index, layer).IsAir)
            {
                Set(index, block, layer, source);
                return true;
            }
            return false;
        }
        public bool TryUnset(BlockIndex index, BlockLayer layer, BlockBrokeSource source = BlockBrokeSource.Player)
        {
            Block toRemoveBlock = Get(index, layer);
            if (!toRemoveBlock.IsAir)
            {
                Set(index, Block.Air, layer, source);
                return true;
            }
            return false;
        }
    }

}