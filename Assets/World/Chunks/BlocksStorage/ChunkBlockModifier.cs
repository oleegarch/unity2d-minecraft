using System.Collections.Generic;
using World.Blocks;
using World.Chunks.BlocksStorage;
using World.Chunks.BlocksStorage.Storages;

namespace World.Chunks
{
    // IBlockAccessor — минимальный интерфейс чтения
    public interface IChunkBlockAccessor
    {
        public Block Get(BlockIndex index, BlockLayer layer);
        public Block Get(byte x, byte y, BlockLayer layer);
    }

    // IChunkBlockModifier — чтение + модификация + события
    public interface IChunkBlockModifier : IChunkBlockAccessor
    {
        public ChunkBlockEvents Events { get; }
        public void Set(BlockIndex index, Block block, BlockLayer layer);
        public void Set(byte x, byte y, ushort blockId, BlockLayer layer);
        public bool TrySet(BlockIndex index, Block block, BlockLayer layer);
        public bool TryUnset(BlockIndex index, BlockLayer layer);
    }
    public class ChunkBlockModifier : IChunkBlockModifier
    {
        private readonly Dictionary<BlockLayer, IBlockLayerStorage> _blockLayers;
        public ChunkBlockEvents Events { get; } = new();

        public ChunkBlockModifier(Dictionary<BlockLayer, IBlockLayerStorage> blockLayers)
        {
            _blockLayers = blockLayers;
        }

        public Block Get(BlockIndex index, BlockLayer layer) => _blockLayers[layer].Get(index);
        public Block Get(byte x, byte y, BlockLayer layer) => Get(new BlockIndex(x, y), layer);

        public void Set(BlockIndex index, Block block, BlockLayer layer)
        {
            _blockLayers[layer].Set(index, block);
            Events.InvokeBlockSet(index, block, layer);
        }

        public void Set(byte x, byte y, ushort blockId, BlockLayer layer)
        {
            Set(new BlockIndex(x, y), new Block(blockId), layer);
        }

        public bool TrySet(BlockIndex index, Block block, BlockLayer layer)
        {
            if (Get(index, layer).IsAir())
            {
                Set(index, block, layer);
                return true;
            }
            return false;
        }

        public bool TryUnset(BlockIndex index, BlockLayer layer)
        {
            Block toRemoveBlock = Get(index, layer);
            if (!toRemoveBlock.IsAir())
            {
                Set(index, Block.Air, layer);
                Events.InvokeBlockBroken(index, toRemoveBlock, layer);
                return true;
            }
            return false;
        }
    }

}