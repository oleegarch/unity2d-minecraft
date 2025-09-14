using World.Blocks;
using World.Chunks.Blocks.Storages;

namespace World.Chunks
{
    // IBlockAccessor — минимальный интерфейс чтения
    public interface IChunkBlockAccessor
    {
        /// <summary>Получить сам интерфейс слоя блоков</summary>
        public IBlockLayerStorage GetLayer(BlockLayer layer);
        /// <summary>Получить блок или воздух если его нет в конкретном слое блоков</summary>
        public Block Get(BlockIndex index, BlockLayer layer);
    }

    // IChunkBlockModifier — чтение + модификация + события
    public interface IChunkBlockModifier : IChunkBlockAccessor
    {
        /// <summary>
        /// Установить блок без событий об этом.
        /// Используется когда мир только создаётся в процессе генерации чанка или применения изменений из хранилища.
        /// </summary>
        public void SetSilent(BlockIndex index, Block block, BlockLayer layer);
        /// <summary>Установить блок, либо его удалить(если в аргументах блок == воздух) + выслать событие об этом</summary>
        public void Set(BlockIndex index, Block block, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player);
        /// <summary>Если блок == воздух, то только тогда произведём установку блока и вернём true</summary>
        public bool TrySet(BlockIndex index, Block block, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player);
        /// <summary>Если блок != воздух, то только тогда попытаемся удалить блок(или переопределить на возвух)</summary>
        public bool TryUnset(BlockIndex index, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player);
    }
    public class ChunkBlockModifier : IChunkBlockModifier
    {
        private readonly IBlockLayerStorage[] _blockLayers;
        private readonly ChunkBlockEvents _events;

        public ChunkBlockModifier(ChunkBlockEvents events, IBlockLayerStorage[] blockLayers)
        {
            _events = events;
            _blockLayers = blockLayers;
        }
        
        public IBlockLayerStorage GetLayer(BlockLayer layer) => _blockLayers[(byte)layer];
        public Block Get(BlockIndex index, BlockLayer layer) => _blockLayers[(byte)layer].Get(index);

        public void SetSilent(BlockIndex index, Block block, BlockLayer layer)
        {
            if (block.IsAir)
            {
                _blockLayers[(byte)layer].Remove(index);
            }
            else
            {
                _blockLayers[(byte)layer].Set(index, block);
            }
        }
        public void Set(BlockIndex index, Block block, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player)
        {
            if (block.IsAir)
            {
                Block oldBlock = Get(index, layer);
                _blockLayers[(byte)layer].Remove(index);
                _events.InvokeBlockBroken(index, oldBlock, layer, source);
            }
            else
            {
                _blockLayers[(byte)layer].Set(index, block);
                _events.InvokeBlockSet(index, block, layer, source);
            }
        }

        public bool TrySet(BlockIndex index, Block block, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player)
        {
            if (Get(index, layer).IsAir)
            {
                Set(index, block, layer, source);
                return true;
            }
            return false;
        }
        public bool TryUnset(BlockIndex index, BlockLayer layer, BlockUpdateSource source = BlockUpdateSource.Player)
        {
            Block toRemoveBlock = Get(index, layer);
            if (!toRemoveBlock.IsAir)
            {
                Set(index, Block.Air, layer, source);
                return true;
            }
            return false;
        }
    }

}