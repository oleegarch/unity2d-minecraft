using System;

namespace World.Inventories
{
    /// <summary>
    /// События инвентаря: уведомление об изменении конкретного слота.
    /// </summary>
    public sealed class InventoryEvents
    {
        /// <summary> Вызывается, когда содержимое слота изменилось. </summary>
        public event Action<int, ItemStack> SlotChanged;

        internal void InvokeSlotChanged(int slot, ItemStack newValue) => SlotChanged?.Invoke(slot, newValue);
    }
}