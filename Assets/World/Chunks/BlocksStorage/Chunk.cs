using System.Collections.Generic;
using World.Blocks;

namespace World.Chunks.BlocksStorage
{
    public class Chunk
    {
        public readonly ChunkIndex Index;
        public readonly byte Size;

        private readonly Dictionary<BlockLayer, IBlockLayerStorage> _layers;
        private readonly ChunkRenderData _renderData;

        public Chunk(ChunkIndex index, byte size)
        {
            Index = index;
            Size = size;

            _renderData = new ChunkRenderData();
            _layers = new()
            {
                [BlockLayer.Main] = new ArrayBlockStorage(size),
                [BlockLayer.Behind] = new ArrayBlockStorage(size),
                [BlockLayer.Front] = new SparseBlockStorage()
            };
        }

        public Block GetBlock(BlockIndex index, BlockLayer layer) =>
            _layers[layer].Get(index);
        public Block GetBlock(byte x, byte y, BlockLayer layer) =>
            _layers[layer].Get(x, y);

        public void SetBlock(BlockIndex index, Block block, BlockLayer layer) =>
            _layers[layer].Set(index, block);
        public void SetBlock(byte x, byte y, ushort blockId, BlockLayer layer) =>
            _layers[layer].Set(x, y, blockId);

        public bool TrySetBlock(BlockIndex index, Block block, BlockLayer layer)
        {
            if (GetBlock(index, layer).IsAir())
            {
                SetBlock(index, block, layer);
                return true;
            }
            return false;
        }

        public bool TryUnsetBlock(BlockIndex index, BlockLayer layer)
        {
            if (!GetBlock(index, layer).IsAir())
            {
                SetBlock(index, Block.Air, layer);
                return true;
            }
            return false;
        }

        public bool TryGetBlockIndex(WorldPosition worldPosition, out BlockIndex blockIndex)
        {
            if (worldPosition.ToChunkIndex(Size) == Index)
            {
                blockIndex = worldPosition.ToBlockIndex(Size);
                return true;
            }

            blockIndex = BlockIndex.Zero;
            return false;
        }

        public bool TryGetBlockRenderId(BlockIndex index, out ushort renderId, out bool isBehind)
        {
            Block main = GetBlock(index, BlockLayer.Main);
            renderId = main.Id;
            isBehind = main.IsAir();

            if (isBehind)
            {
                Block behind = GetBlock(index, BlockLayer.Behind);

                if (behind.IsAir())
                    return false;

                isBehind = _renderData.ShouldDarken(index);
                renderId = behind.Id;
            }

            return true;
        }

        public void DisableDarkeningToBlockBehind(BlockIndex index) =>
            _renderData.DisableDarkening(index);
    }
}