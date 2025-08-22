using World.Blocks;

namespace World.Chunks.BlocksStorage.Storages
{
    public interface IBlockLayerStorage
    {
        public Block Get(BlockIndex index);
        public void Set(BlockIndex index, Block block);
    }
}