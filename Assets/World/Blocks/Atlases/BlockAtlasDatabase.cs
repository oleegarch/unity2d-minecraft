using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace World.Blocks.Atlases
{
    [CreateAssetMenu(menuName = "Blocks/New BlockAtlasDatabase")]
    public class BlockAtlasDatabase : ScriptableObject
    {
        public List<BlockAtlasInfo> atlases;

        private Dictionary<BlockAtlasCategory, BlockAtlasInfo> _byCategory;

        private void OnEnable()
        {
            _byCategory = atlases.ToDictionary(b => b.Category);
        }

        public BlockAtlasInfo Get(BlockAtlasCategory category) => _byCategory[category];
    }
}