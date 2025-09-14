using System.Collections;
using System.Collections.Generic;
using World.Blocks;

namespace World.Chunks.Blocks.Storages
{
    public class SparseBlockStorage : IBlockLayerStorage
    {
        private readonly Dictionary<BlockIndex, Block> _blocks = new();

        public Block Get(BlockIndex index)
        {
            if (_blocks.TryGetValue(index, out var block))
            {
                return block;
            }

            return Block.Air;
        }

        public void Set(BlockIndex index, Block block) => _blocks[index] = block;
        public void Remove(BlockIndex index) => _blocks.Remove(index);

        // Перечисление пар (индекс, блок)
        public IEnumerator<KeyValuePair<BlockIndex, Block>> GetEnumerator() => _blocks.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Дополнительное перечисление только блоков
        public IEnumerable<Block> Blocks
        {
            get
            {
                foreach (var kv in _blocks)
                    yield return kv.Value;
            }
        }
    }
}