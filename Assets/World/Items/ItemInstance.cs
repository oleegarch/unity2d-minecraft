using System;

namespace World.Items
{
    [Serializable]
    public class ItemInstance : IEquatable<ItemInstance>
    {
        public ushort Id;

        public ItemInstance(ushort id)
        {
            Id = id;
        }

        public ItemInfo GetItemInfo(ItemDatabase itemDatabase)
        {
            return itemDatabase.Get(Id);
        }

        public ItemInstance Clone()
        {
            return new ItemInstance(Id);
        }

        // Правило, можно ли стэковать два экземпляра. По умолчанию: только если состояния равны.
        public virtual bool CanStackWith(ItemInstance other)
        {
            return Equals(other);
        }

        public bool Equals(ItemInstance other)
        {
            return other != null && Id == other.Id;
        }
    }
}