using System;
using World.Items;

namespace World.Inventories
{
    public sealed class ItemStack : IEquatable<ItemStack>
    {
        public ItemInfo Item { get; private set; }
        public int Count { get; private set; }

        public bool IsEmpty => Item == null || Count <= 0;
        public int SpaceLeft => Item == null ? 0 : Item.MaxStack - Count;

        public ItemStack(ItemInfo item, int count = 1)
        {
            if (item == null)
            {
                if (count != 0) throw new InvalidOperationException("Can't create non-empty stack with null item.");
                Item = null;
                Count = 0;
                return;
            }

            Item = item;
            Count = Math.Max(0, count);
            if (Count == 0) Item = null;
        }

        // Пустой стек-одиночка
        public static ItemStack Empty => new(null, 0);

        public ItemStack Clone() => new(Item, Count);

        public bool CanStackWith(ItemStack other)
        {
            if (other == null) return false;
            if (IsEmpty || other.IsEmpty) return true;
            return Item.Id == other.Item.Id;
        }

        public int Add(int amount)
        {
            if (amount <= 0 || Item == null) return 0;
            int can = Math.Min(amount, Item.MaxStack - Count);
            Count += can;
            return can;
        }

        public int Remove(int amount)
        {
            if (amount <= 0 || IsEmpty) return 0;
            int rem = Math.Min(amount, Count);
            Count -= rem;
            if (Count == 0) Item = null;
            return rem;
        }

        public bool Equals(ItemStack other)
        {
            if (other == null) return IsEmpty;
            if (IsEmpty && other.IsEmpty) return true;
            return Item?.Id == other.Item?.Id && Count == other.Count;
        }

        public override string ToString() => IsEmpty ? "(empty)" : $"{Item.Id} x{Count}";
    }
}