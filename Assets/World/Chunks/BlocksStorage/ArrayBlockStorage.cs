using World.Blocks;

namespace World.Chunks.BlocksStorage
{
    public class ArrayBlockStorage : IBlockLayerStorage
    {
        public Block[,] Blocks;

        public ArrayBlockStorage(byte size) => Blocks = new Block[size, size];

        public Block Get(BlockIndex index) => Blocks[index.x, index.y];
        public Block Get(byte x, byte y) => Blocks[x, y];
        public void Set(BlockIndex index, Block block) => Blocks[index.x, index.y] = block;
        public void Set(byte x, byte y, ushort blockId) => Blocks[x, y] = new Block(blockId);
    }
}