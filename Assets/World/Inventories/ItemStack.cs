using System;
using World.Items;

namespace World.Inventories
{
    /// <summary>
    /// Представляет стек предметов.
    /// </summary>
    public sealed class ItemStack : IEquatable<ItemStack>
    {
        public static ItemStack Empty => new(null, 0, 0);

        public ItemInstance Item { get; private set; }
        public int Quantity { get; private set; }
        public int MaxStack { get; private set; }

        public bool IsEmpty => Item == null || Quantity <= 0;
        public int SpaceRemaining => Item == null ? 0 : MaxStack - Quantity;

        public ItemStack(ItemInstance item, int maxStack, int quantity = 1)
        {
            if (item == null)
            {
                if (quantity != 0) throw new InvalidOperationException("Нельзя создать непустой стек с null item.");
                Item = null;
                Quantity = 0;
                MaxStack = 0;
                return;
            }

            Item = item;
            Quantity = Math.Max(0, quantity);
            MaxStack = maxStack;
            if (Quantity == 0) Item = null;
        }

        public ItemStack Clone() => new(Item?.Clone(), MaxStack, Quantity);

        public bool CanStackWith(ItemStack other)
        {
            if (other == null) return false;
            if (IsEmpty || other.IsEmpty) return true;
            return Item.CanStackWith(other.Item);
        }

        public int Add(int amount)
        {
            if (amount <= 0 || Item == null) return 0;
            int can = Math.Min(amount, MaxStack - Quantity);
            Quantity += can;
            return can;
        }

        public int Remove(int amount)
        {
            if (amount <= 0 || IsEmpty) return 0;
            int rem = Math.Min(amount, Quantity);
            Quantity -= rem;
            if (Quantity == 0) Item = null;
            return rem;
        }

        public bool Equals(ItemStack other)
        {
            return (
                Item != null &&
                other.Item != null &&
                Item.Equals(other.Item) &&
                Quantity == other.Quantity
            );
        }

        public override string ToString() => IsEmpty ? "(пусто)" : $"{Item?.ItemId} x{Quantity}";
    }
}