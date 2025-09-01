using UnityEngine;
using World.Blocks;
using World.Blocks.Atlases;
using World.Items;

namespace World.Chunks.Generator
{
    public abstract class ChunkGeneratorConfig : ScriptableObject
    {
        [SerializeField] protected BlockDatabase _blockDatabase;
        [SerializeField] protected BlockAtlasDatabase _blockAtlasDatabase;
        [SerializeField] protected ItemDatabase _itemDatabase;
        [SerializeField] protected ItemCategoryDatabase _itemCategoryDatabase;
        [SerializeField] protected byte _chunkSize = 16;

        public BlockDatabase BlockDatabase => _blockDatabase;
        public BlockAtlasDatabase BlockAtlasDatabase => _blockAtlasDatabase;
        public ItemDatabase ItemDatabase => _itemDatabase;
        public ItemCategoryDatabase ItemCategoryDatabase => _itemCategoryDatabase;
        public byte ChunkSize => _chunkSize;

        public abstract IChunkGenerator GetChunkGenerator(int seed);
    }
}