using System.Collections.Generic;
using World.Items;

namespace World.Inventories
{
    public interface IInventory
    {
        public int SlotCount { get; }
        public InventoryEvents Events { get; }
        public IReadOnlyList<ItemStack> GetAllSlots();
        public IReadOnlyList<ItemStack> GetAllNotEmptySlots();
        public ItemStack GetSlot(int index);

        bool Has(ItemStack stack, int slotIndex);
        bool Has(ItemStack stack);

        public bool TryAdd(ItemStack stack, out int remainder);
        public bool TryRemove(int slotIndex, int amount, out ItemStack removed);
        public bool Move(int fromIndex, int toIndex, int amount = int.MaxValue);
        public bool MoveTo(Inventory target, int fromIndex, int toIndex, int amount = int.MaxValue);
        public void ReplaceSlot(int index, ItemStack newStack);

        public void Clear();
    }
}