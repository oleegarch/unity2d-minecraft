using System;

namespace World.Items
{
    [Serializable]
    public class ItemInstance : IEquatable<ItemInstance>
    {
        public ushort ItemId;
        public ushort Id => ItemId;

        public ItemInstance(ushort id)
        {
            ItemId = id;
        }

        public ItemInfo GetItemInfo(ItemDatabase itemDatabase)
        {
            return itemDatabase.Get(ItemId);
        }

        public ItemInstance Clone()
        {
            return new ItemInstance(ItemId);
        }

        // Правило, можно ли стэковать два экземпляра. По умолчанию: только если состояния равны.
        public virtual bool CanStackWith(ItemInstance other)
        {
            return Equals(other);
        }

        public bool Equals(ItemInstance other)
        {
            return other != null && ItemId == other.ItemId;
        }
    }
}