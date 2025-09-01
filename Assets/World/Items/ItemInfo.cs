using UnityEngine;

namespace World.Items
{
    [CreateAssetMenu(menuName = "Items/New ItemInfo")]
    public class ItemInfo : ScriptableObject
    {
        public ushort Id;
        public string Name;
        public ushort BlockId;
        public Sprite Sprite;
        public ItemCategory Category;
        public ushort MaxStack = 100;
    }
}