using World.Blocks;

namespace World.Chunks.BlocksStorage.Storages
{
    public interface IBlockLayerStorage
    {
        public Block Get(BlockIndex index);
        public Block Get(byte x, byte y);
        public void Set(BlockIndex index, Block block);
        public void Set(byte x, byte y, ushort blockId);
    }
}