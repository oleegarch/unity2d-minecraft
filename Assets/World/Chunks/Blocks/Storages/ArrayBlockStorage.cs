using System.Collections;
using System.Collections.Generic;
using World.Blocks;

namespace World.Chunks.Blocks.Storages
{
    public class ArrayBlockStorage : IBlockLayerStorage
    {
        private readonly Block[] _blocks;
        private readonly byte _size;

        public ArrayBlockStorage(byte chunkSize)
        {
            _size = chunkSize;
            _blocks = new Block[chunkSize * chunkSize];
        }

        private int ToLinear(BlockIndex index) => index.y * _size + index.x;

        public Block Get(BlockIndex index) => _blocks[ToLinear(index)];
        public void Set(BlockIndex index, Block block) => _blocks[ToLinear(index)] = block;
        public void Remove(BlockIndex index) => _blocks[ToLinear(index)] = Block.Air;

        // Перечисление пар (индекс, блок)
        public IEnumerator<KeyValuePair<BlockIndex, Block>> GetEnumerator()
        {
            for (byte y = 0; y < _size; y++)
            {
                for (byte x = 0; x < _size; x++)
                {
                    var index = new BlockIndex(x, y);
                    yield return new KeyValuePair<BlockIndex, Block>(index, Get(index));
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Дополнительное перечисление только блоков
        public IEnumerable<Block> Blocks
        {
            get
            {
                foreach (var kv in this)
                    yield return kv.Value;
            }
        }
    }
}