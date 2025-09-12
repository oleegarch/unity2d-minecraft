using World.Blocks;

namespace World.Chunks.Blocks.Storages
{
    public interface IBlockLayerStorage
    {
        public Block Get(BlockIndex index);
        public void Set(BlockIndex index, Block block);
    }
}