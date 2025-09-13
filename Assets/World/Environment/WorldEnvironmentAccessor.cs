using UnityEngine;
using World.Blocks;
using World.Blocks.Atlases;
using World.Chunks.Generator;
using World.Entities;
using World.Items;

namespace World.Chunks
{
    public class WorldEnvironmentAccessor : MonoBehaviour
    {
        [SerializeField] private WorldEnvironment _environment;

        public WorldEnvironment Environment => _environment;
        public BlockDatabase BlockDatabase => _environment.BlockDatabase;
        public BlockAtlasDatabase BlockAtlasDatabase => _environment.BlockAtlasDatabase;
        public ItemDatabase ItemDatabase => _environment.ItemDatabase;
        public ItemCategoryDatabase ItemCategoryDatabase => _environment.ItemCategoryDatabase;
        public EntityDatabase EntityDatabase => _environment.EntityDatabase;
    }
}