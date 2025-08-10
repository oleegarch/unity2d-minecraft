using World.Blocks;

namespace World.Chunks.BlocksStorage.Storages
{
    public class ArrayBlockStorage : IBlockLayerStorage
    {
        private readonly Block[,] _blocks;

        public ArrayBlockStorage(byte size) => _blocks = new Block[size, size];

        public Block Get(BlockIndex index) => _blocks[index.x, index.y];
        public Block Get(byte x, byte y) => _blocks[x, y];
        public void Set(BlockIndex index, Block block) => _blocks[index.x, index.y] = block;
        public void Set(byte x, byte y, ushort blockId) => _blocks[x, y] = new Block(blockId);
    }
}