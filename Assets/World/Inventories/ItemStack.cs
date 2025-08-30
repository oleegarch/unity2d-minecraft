using System;
using World.Items;

namespace World.Inventories
{
    public sealed class ItemStack : IEquatable<ItemStack>
    {
        public static ItemStack Empty => new(null, 0, 0);

        public ItemInstance Instance { get; private set; }
        public int Count { get; private set; }
        public int MaxCount { get; private set; }

        public bool IsEmpty => Instance == null || Count <= 0;
        public int SpaceLeft => Instance == null ? 0 : MaxCount - Count;

        public ItemStack(ItemInstance instance, int maxCount, int count = 1)
        {
            if (instance == null)
            {
                if (count != 0) throw new InvalidOperationException("Can't create non-empty stack with null instance.");
                Instance = null;
                Count = 0;
                return;
            }

            Instance = instance;
            Count = Math.Max(0, count);
            MaxCount = maxCount;
            if (Count == 0) Instance = null;
        }

        public ItemStack Clone() => new(Instance?.Clone(), MaxCount, Count);

        public bool CanStackWith(ItemStack other)
        {
            if (other == null) return false;
            if (IsEmpty || other.IsEmpty) return true;
            return Instance.CanStackWith(other.Instance);
        }

        public int Add(int amount)
        {
            if (amount <= 0 || Instance == null) return 0;
            int can = Math.Min(amount, MaxCount - Count);
            Count += can;
            return can;
        }

        public int Remove(int amount)
        {
            if (amount <= 0 || IsEmpty) return 0;
            int rem = Math.Min(amount, Count);
            Count -= rem;
            if (Count == 0) Instance = null;
            return rem;
        }

        public bool Equals(ItemStack other)
        {
            return (
                Instance != null &&
                other.Instance != null &&
                Instance.Equals(other.Instance) &&
                Count == other.Count
            );
        }

        public override string ToString() => IsEmpty ? "(empty)" : $"{Instance?.ItemId} x{Count}";
    }
}