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

        public ItemStack(ushort ItemId, int maxStack, int quantity = 1) : this(new ItemInstance(ItemId), maxStack, quantity) { }
        public ItemStack(ItemInfo info, int quantity = 1) : this(new ItemInstance(info.Id), info.MaxStack, quantity) { }
        public ItemStack(ItemInstance item, int maxStack, int quantity = 1)
        {
            if (quantity > maxStack) throw new ArgumentOutOfRangeException("Попытка создать ItemStack где Quantity уже больше чем MaxStack!");

            if (item == null)
            {
                if (quantity != 0) throw new InvalidOperationException("Нельзя создать непустой ItemStack с null item!");
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
            if (amount <= 0 || IsEmpty) return 0;
            int added = Math.Min(amount, MaxStack - Quantity);
            Quantity += added;
            return added;
        }
        public int Remove(int amount)
        {
            if (amount <= 0 || IsEmpty) return 0;
            int removed = Math.Min(amount, Quantity);
            Quantity -= removed;
            if (Quantity == 0) Item = null;
            return removed;
        }
        public ItemStack SetQuantity(int amount)
        {
            Quantity = amount;
            if (Quantity <= 0) Item = null;
            return this;
        }

        public void MakeEmpty()
        {
            Item = null;
            MaxStack = 0;
            Quantity = 0;
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

        public override string ToString() => IsEmpty ? "пустой" : $"Item({Item?.Id},Max={MaxStack},x={Quantity})";
    }
}