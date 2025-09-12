using World.Blocks;

namespace World.Chunks.Blocks.Storages
{
    public class ArrayBlockStorage : IBlockLayerStorage
    {
        private readonly Block[] _blocks;
        private readonly int _size;

        public ArrayBlockStorage(int size)
        {
            _size = size;
            _blocks = new Block[size * size];
        }

        private int ToLinear(BlockIndex index) => index.y * _size + index.x;

        public Block Get(BlockIndex index) => _blocks[ToLinear(index)];
        public void Set(BlockIndex index, Block block) => _blocks[ToLinear(index)] = block;
    }
}