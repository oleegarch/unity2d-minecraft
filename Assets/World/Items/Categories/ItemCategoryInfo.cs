using UnityEngine;

namespace World.Items
{
    public enum ItemCategory
    {
        BuildingBlocks,
        NaturalBlocks,
        FunctionalBlocks
    }

    [CreateAssetMenu(menuName = "Items/New ItemCategoryInfo")]
    public class ItemCategoryInfo : ScriptableObject
    {
        public ItemCategory Category;
        public Sprite Sprite;
        public string Title;
        public bool IsCategoryForBlocks;
    }
}