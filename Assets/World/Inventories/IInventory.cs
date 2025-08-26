using System.Collections.Generic;
using World.Items;

namespace World.Inventories
{
    public interface IInventory
    {
        public int SlotCount { get; }
        public IReadOnlyList<ItemStack> GetAllSlots();
        public ItemStack GetSlot(int index);
        public void ReplaceSlot(int index, ItemStack newStack);

        public bool TryAdd(ItemStack stack, out int remainder);
        public bool TryAdd(ItemInfo stack);
        public bool TryRemove(int slotIndex, int amount, out ItemStack removed);
        public bool Move(int fromIndex, int toIndex, int amount = int.MaxValue);

        public void Clear();
    }
}