using System.Linq;
using UnityEngine;
using World.Blocks;
using World.Blocks.Atlases;
using World.Entities;
using World.Items;

namespace World.Chunks.Generator
{
    [CreateAssetMenu(menuName = "WorldConfigs/WorldConfig")]
    public class WorldConfig : ScriptableObject
    {
        [SerializeField] protected BlockDatabase _blockDatabase;
        [SerializeField] protected BlockAtlasDatabase _blockAtlasDatabase;
        [SerializeField] protected ItemDatabase _itemDatabase;
        [SerializeField] protected ItemCategoryDatabase _itemCategoryDatabase;
        [SerializeField] protected EntityDatabase _entityDatabase;
        [SerializeField] protected ChunkGeneratorConfig[] _chunkGeneratorConfigs;

        public BlockDatabase BlockDatabase => _blockDatabase;
        public BlockAtlasDatabase BlockAtlasDatabase => _blockAtlasDatabase;
        public ItemDatabase ItemDatabase => _itemDatabase;
        public ItemCategoryDatabase ItemCategoryDatabase => _itemCategoryDatabase;
        public EntityDatabase EntityDatabase => _entityDatabase;

        public ChunkGeneratorConfig GetChunkGeneratorConfig(string generatorName)
        {
            return _chunkGeneratorConfigs.First(c => c.Name == generatorName);
        }
        public IChunkGenerator GetChunkGenerator(string generatorName, int seed)
        {
            ChunkGeneratorConfig generatorConfig = GetChunkGeneratorConfig(generatorName);
            return generatorConfig.GetChunkGenerator(this, seed);
        }
    }
}