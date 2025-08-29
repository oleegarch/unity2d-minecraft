using System.Collections.Generic;
using World.Items;

namespace World.Inventories
{
    public interface IInventory
    {
        public int SlotCount { get; }
        public InventoryEvents Events { get; }
        public IReadOnlyList<ItemStack> GetAllSlots();
        public ItemStack GetSlot(int index);
        public void ReplaceSlot(int index, ItemStack newStack);

        bool Has(ItemStack stack, int slotIndex);
        bool Has(ItemInfo item, int slotIndex, int count = 1);
        bool Has(ItemStack stack);
        bool Has(ItemInfo item, int count = 1);

        public bool TryAdd(ItemStack stack, out int remainder);
        public bool TryAdd(ItemInfo stack, int count, out int remainder);
        public bool TryRemove(int slotIndex, int amount, out ItemStack removed);
        public bool Move(int fromIndex, int toIndex, int amount = int.MaxValue);
        public bool MoveTo(Inventory target, int fromIndex, int toIndex, int amount = int.MaxValue);

        public void Clear();
    }
}