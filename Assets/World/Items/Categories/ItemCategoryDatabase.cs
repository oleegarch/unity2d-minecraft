using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace World.Items
{
    [CreateAssetMenu(menuName = "Items/New ItemCategoryDatabase")]
    public class ItemCategoryDatabase : ScriptableObject
    {
        public List<ItemCategoryInfo> categories = new();

        public ItemCategoryInfo Get(ItemCategory category)
        {
            return categories.First(categoryInfo => categoryInfo.Category == category);
        }
    }
}