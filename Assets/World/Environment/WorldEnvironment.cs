using System.Linq;
using UnityEngine;
using World.Blocks;
using World.Blocks.Atlases;
using World.Entities;
using World.Items;

namespace World.Chunks.Generator
{
    [CreateAssetMenu(menuName = "Environments/WorldEnvironment")]
    public class WorldEnvironment : ScriptableObject
    {
        [SerializeField] protected BlockDatabase _blockDatabase;
        [SerializeField] protected BlockAtlasDatabase _blockAtlasDatabase;
        [SerializeField] protected ItemDatabase _itemDatabase;
        [SerializeField] protected ItemCategoryDatabase _itemCategoryDatabase;
        [SerializeField] protected EntityDatabase _entityDatabase;
        [SerializeField] protected WorldGeneratorConfig[] _worldGeneratorConfigs;

        public BlockDatabase BlockDatabase => _blockDatabase;
        public BlockAtlasDatabase BlockAtlasDatabase => _blockAtlasDatabase;
        public ItemDatabase ItemDatabase => _itemDatabase;
        public ItemCategoryDatabase ItemCategoryDatabase => _itemCategoryDatabase;
        public EntityDatabase EntityDatabase => _entityDatabase;

        public WorldGeneratorConfig[] WorldGeneratorConfigs => _worldGeneratorConfigs;
        public string[] WorldGeneratorNames => _worldGeneratorConfigs.Select(c => c.Name).ToArray();

        public WorldGeneratorConfig GetWorldGeneratorConfig(string generatorName)
        {
            return _worldGeneratorConfigs.First(c => c.Name == generatorName);
        }
        public IWorldGenerator GetWorldGenerator(string generatorName, int seed)
        {
            WorldGeneratorConfig generatorConfig = GetWorldGeneratorConfig(generatorName);
            return generatorConfig.GetWorldGenerator(this, seed);
        }
    }
}