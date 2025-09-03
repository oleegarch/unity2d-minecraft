using System.Collections.Generic;

namespace World.Inventories
{
    public interface IInventory
    {
        /// <summary>Количество слотов в инвентаре.</summary>
        public int Capacity { get; }

        /// <summary>События, которые испускает инвентарь.</summary>
        public InventoryEvents Events { get; }

        /// <summary>Возвращает копию конкретного слота по индексу.</summary>
        public ItemStack GetSlot(int slotIndex);
        /// <summary>Возвращает поверхностную копию всех слотов (каждый ItemStack клонируется).</summary>
        public IReadOnlyList<ItemStack> GetAllSlots();
        /// <summary>Возвращает только непустые слоты (каждый ItemStack клонируется).</summary>
        public IReadOnlyList<ItemStack> GetNonEmptySlots();

        /// <summary>Проверяет, содержит ли указанный слот как минимум запрошенное количество того же предмета.</summary>
        public bool Has(ushort itemId, int amount, int slotIndex);
        public bool Has(ItemStack requested, int slotIndex);

        /// <summary>Проверяет, содержит ли инвентарь как минимум запрошенное количество предмета по всем слотам.</summary>
        public bool Has(ushort itemId, int amount);
        public bool Has(ItemStack requested);

        /// <summary>Пытается добавить стек в инвентарь. Возвращает true, если весь стек помещён.</summary>
        public bool TryAdd(ItemStack stack);

        /// <summary>Пытается удалить до <paramref name="amount"/> из указанного слота. Возвращает true, если что-то было удалено и отдаёт удалённый стек.</summary>
        public bool TryRemove(int slotIndex, int amount, out ItemStack removed);
        /// <summary>Пытается удалить указанный слот. Возвращает true, если что-то было удалено и отдаёт удалённый стек.</summary>
        public bool Remove(int slotIndex, out ItemStack removed);

        /// <summary>Пытается забрать <paramref name="amount"/> предметов с идентификатором <paramref name="itemId"/>. Если их не хватает — ничего не заберёт и вернёт false.</summary>
        public bool Take(ushort itemId, int needed);
        public bool Take(ItemStack stack);

        /// <summary>Переместить предметы внутри этого инвентаря из одного слота в другой.</summary>
        public bool Move(int fromIndex, int toIndex, int amount = int.MaxValue);

        /// <summary>Переместить предметы из этого инвентаря в другой инвентарь.</summary>
        public bool MoveTo(Inventory target, int fromSlotIndex, int toSlotIndex, int amount = int.MaxValue);

        /// <summary>Заменить содержимое слота валидированным стеком.</summary>
        public void Replace(int slotIndex, ItemStack newStack, out ItemStack old);

        /// <summary>Очистить инвентарь (установить все слоты в Empty) и уведомить слушателей.</summary>
        public void Clear();
    }
}