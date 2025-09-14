using System.Collections;
using System.Collections.Generic;
using World.Blocks;

namespace World.Chunks.Blocks.Storages
{
    public interface IBlockLayerStorage : IEnumerable<KeyValuePair<BlockIndex, Block>>
    {
        /// <summary>Получить блок</summary>
        public Block Get(BlockIndex index);
        /// <summary>Заполняет/переопределяет блок, даже если это воздух</summary>
        public void Set(BlockIndex index, Block block);
        /// <summary>Удаляет/переопределяет блок на возвух</summary>
        public void Remove(BlockIndex index);
    }
}