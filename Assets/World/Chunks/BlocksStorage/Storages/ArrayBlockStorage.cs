using World.Blocks;

namespace World.Chunks.BlocksStorage.Storages
{
    public class ArrayBlockStorage : IBlockLayerStorage
    {
        private readonly Block[,] _blocks;

        public ArrayBlockStorage(byte size) => _blocks = new Block[size, size];

        public Block Get(BlockIndex index) => _blocks[index.x, index.y];
        public void Set(BlockIndex index, Block block) => _blocks[index.x, index.y] = block;
    }
}