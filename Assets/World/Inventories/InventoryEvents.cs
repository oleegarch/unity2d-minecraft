using System;

namespace World.Inventories
{
    /// <summary>
    /// Аргументы события SlotChanged.
    /// </summary>
    public sealed class SlotChangedEventArgs : EventArgs
    {
        public int SlotIndex { get; }
        public ItemStack NewValue { get; }

        public SlotChangedEventArgs(int slotIndex, ItemStack newValue)
        {
            SlotIndex = slotIndex;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// Простой контейнер для событий инвентаря. Экспортирует типизированное событие SlotChanged.
    /// </summary>
    public sealed class InventoryEvents
    {
        public event EventHandler<SlotChangedEventArgs> SlotChanged;

        internal void InvokeSlotChanged(int slot, ItemStack newValue) => SlotChanged?.Invoke(this, new SlotChangedEventArgs(slot, newValue));
    }
}