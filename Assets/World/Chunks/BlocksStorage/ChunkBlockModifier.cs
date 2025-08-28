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
        public void Set(BlockIndex index, Block block, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player);
        public bool TrySet(BlockIndex index, Block block, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player);
        public bool TryUnset(BlockIndex index, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player);
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
        public void Set(BlockIndex index, Block block, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player)
        {
            if (block.IsAir)
            {
                Block oldBlock = Get(index, layer);
                _blockLayers[(int)layer].Set(index, block);
                Events.InvokeBlockBroken(index, oldBlock, layer, source);
            }
            else
            {
                _blockLayers[(int)layer].Set(index, block);
                Events.InvokeBlockSet(index, block, layer, source);
            }
        }

        public bool TrySet(BlockIndex index, Block block, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player)
        {
            if (Get(index, layer).IsAir)
            {
                Set(index, block, layer, source);
                return true;
            }
            return false;
        }
        public bool TryUnset(BlockIndex index, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player)
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