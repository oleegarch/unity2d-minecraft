using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace World.Inventories
{
    /// <summary>
    /// Базовая реализация IInventory. Управляет слотами и общими операциями (добавление/удаление/перемещение).
    /// </summary>
    public abstract class Inventory : IInventory
    {
        // внутреннее хранилище слотов. Protected чтобы производные инвентари могли обращаться напрямую при необходимости.
        protected readonly ItemStack[] slots;

        /// <inheritdoc/>
        public int Capacity => slots.Length;

        /// <inheritdoc/>
        public InventoryEvents Events { get; }

        protected Inventory(int capacity)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            slots = Enumerable.Repeat(ItemStack.Empty, capacity).ToArray();
            Events = new InventoryEvents();
        }

        #region Валидаторы
        private int EnsureIndexInRange(int index)
        {
            if (index < 0 || index >= slots.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return index;
        }

        private void EnsureValidStack(ItemStack stack)
        {
            if (stack == null || stack.IsEmpty || stack.Item == null || stack.Item.ItemId == 0)
                throw new InvalidDataException(nameof(stack));
        }
        #endregion

        #region Операции получения
        public IReadOnlyList<ItemStack> GetAllSlots() => slots.Select(s => s?.Clone() ?? ItemStack.Empty).ToArray();

        public IReadOnlyList<ItemStack> GetNonEmptySlots() => slots.Where(s => !s.IsEmpty).Select(s => s.Clone()).ToArray();

        public ItemStack GetSlot(int index) => slots[EnsureIndexInRange(index)]?.Clone() ?? ItemStack.Empty;
        #endregion

        #region Проверки наличия
        /// <summary>
        /// Проверяет, что конкретный слот содержит тот же предмет и как минимум запрошенное количество.
        /// </summary>
        public virtual bool Has(ItemStack requested, int slotIndex)
        {
            EnsureValidStack(requested);
            EnsureIndexInRange(slotIndex);

            var slot = slots[slotIndex];
            if (slot.IsEmpty || slot.Item == null) return false;

            return slot.Item.Id == requested.Item.Id && slot.Quantity >= requested.Quantity;
        }

        /// <summary>
        /// Проверяет по всему инвентарю, хватает ли общего количества предмета.
        /// </summary>
        public virtual bool Has(ItemStack requested)
        {
            EnsureValidStack(requested);

            int needed = requested.Quantity;
            var itemId = requested.Item.Id;

            foreach (var slot in slots)
            {
                if (!slot.IsEmpty && slot.Item != null && slot.Item.Id == itemId)
                {
                    needed -= slot.Quantity;
                    if (needed <= 0) return true;
                }
            }

            return false;
        }
        #endregion

        #region Добавление
        /// <summary>
        /// Попытаться добавить стек: сначала заполняем существующие стеки того же типа, затем используем пустые слоты.
        /// remainder — количество, которое не удалось разместить. Возвращает true, если всё поместилось.
        /// </summary>
        public virtual bool TryAdd(ItemStack stack, out int remainder)
        {
            EnsureValidStack(stack);

            int toPlace = stack.Quantity;
            var itemId = stack.Item.Id;

            // Заполняем существующие стеки
            for (int i = 0; i < slots.Length && toPlace > 0; i++)
            {
                var s = slots[i];
                if (!s.IsEmpty && s.Item != null && s.Item.Id == itemId)
                {
                    int added = s.Add(toPlace);
                    if (added > 0) Events.InvokeSlotChanged(i, slots[i].Clone());
                    toPlace -= added;
                }
            }

            // Используем пустые слоты
            for (int i = 0; i < slots.Length && toPlace > 0; i++)
            {
                var s = slots[i];
                if (s.IsEmpty)
                {
                    int put = Math.Min(toPlace, stack.MaxStack);
                    slots[i] = new ItemStack(stack.Item.Clone(), stack.MaxStack, put);
                    Events.InvokeSlotChanged(i, slots[i].Clone());
                    toPlace -= put;
                }
            }

            remainder = toPlace;
            return remainder == 0;
        }
        #endregion

        #region Удаление
        /// <summary>
        /// Попытаться удалить до <paramref name="amount"/> из указанного слота. Возвращает удалённый стек.
        /// </summary>
        public virtual bool TryRemove(int slotIndex, int amount, out ItemStack removed)
        {
            EnsureIndexInRange(slotIndex);

            removed = ItemStack.Empty;
            if (amount <= 0) return false;

            var s = slots[slotIndex];
            if (s.IsEmpty) return false;

            var item = s.Item; // может быть null если повреждённый слот
            int removedCount = s.Remove(amount);

            if (removedCount > 0)
            {
                removed = new ItemStack(item?.Clone(), s.MaxStack, removedCount);
                if (slots[slotIndex].IsEmpty) slots[slotIndex] = ItemStack.Empty;
                Events.InvokeSlotChanged(slotIndex, slots[slotIndex].Clone());
                return true;
            }

            return false;
        }
        #endregion

        #region Перемещение
        /// <summary>
        /// Переместить предметы внутри инвентаря.
        /// - если целевой слот пуст — перенос
        /// - если тот же тип — попытка слить в стек (до max stack)
        /// - если другой тип — swap
        /// </summary>
        public virtual bool Move(int fromIndex, int toIndex, int amount = int.MaxValue)
        {
            EnsureIndexInRange(fromIndex);
            EnsureIndexInRange(toIndex);
            if (fromIndex == toIndex) return false;

            var from = slots[fromIndex];
            var to = slots[toIndex];

            if (from.IsEmpty) return false;

            bool changed = false;

            // Целевой пустой -> перенос
            if (to.IsEmpty)
            {
                int toMove = Math.Min(amount, from.Quantity);
                slots[toIndex] = new ItemStack(from.Item.Clone(), from.MaxStack, toMove);
                from.Remove(toMove);
                if (from.IsEmpty) slots[fromIndex] = ItemStack.Empty;
                changed = true;
            }
            // Тот же тип -> слияние
            else if (!to.IsEmpty && !from.IsEmpty && to.Item != null && from.Item != null && to.Item.Id == from.Item.Id)
            {
                int canMove = Math.Min(amount, from.Quantity);
                int added = Math.Min(canMove, to.MaxStack - to.Quantity);
                if (added > 0)
                {
                    to.Add(added);
                    from.Remove(added);
                    if (from.IsEmpty) slots[fromIndex] = ItemStack.Empty;
                    changed = true;
                }
            }
            // Другой тип -> swap
            else
            {
                slots[fromIndex] = to;
                slots[toIndex] = from;
                changed = true;
            }

            if (changed)
            {
                Events.InvokeSlotChanged(fromIndex, slots[fromIndex].Clone());
                Events.InvokeSlotChanged(toIndex, slots[toIndex].Clone());
            }

            return changed;
        }

        /// <summary>
        /// Переместить предметы из этого инвентаря в целевой инвентарь.
        /// Поведение аналогично Move, но применяется к другому инвентарю.
        /// </summary>
        public virtual bool MoveTo(Inventory target, int fromIndex, int toIndex, int amount = int.MaxValue)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            EnsureIndexInRange(fromIndex);
            target.EnsureIndexInRange(toIndex);

            if (amount <= 0) return false;
            if (target == this) return Move(fromIndex, toIndex, amount);

            var from = slots[fromIndex];
            var to = target.slots[toIndex];

            if (from.IsEmpty) return false;

            bool changed = false;

            // Целевой пустой -> перенос (но не больше max stack)
            if (to.IsEmpty)
            {
                int toMove = Math.Min(amount, from.Quantity);
                var item = from.Item;
                if (item != null) toMove = Math.Min(toMove, from.MaxStack);

                target.slots[toIndex] = new ItemStack(item.Clone(), from.MaxStack, toMove);
                from.Remove(toMove);
                if (from.IsEmpty) slots[fromIndex] = ItemStack.Empty;
                changed = true;
            }
            // Тот же тип -> слияние
            else if (!to.IsEmpty && !from.IsEmpty && to.Item != null && from.Item != null && to.Item.Id == from.Item.Id)
            {
                int canMove = Math.Min(amount, from.Quantity);
                int space = to.MaxStack - to.Quantity;
                int added = Math.Min(canMove, space);
                if (added > 0)
                {
                    to.Add(added);
                    from.Remove(added);
                    if (from.IsEmpty) slots[fromIndex] = ItemStack.Empty;
                    changed = true;
                }
            }
            // Другой тип -> полный обмен стеков между инвентарями
            else
            {
                slots[fromIndex] = to;
                target.slots[toIndex] = from;
                changed = true;
            }

            if (changed)
            {
                Events.InvokeSlotChanged(fromIndex, slots[fromIndex].Clone());
                target.Events.InvokeSlotChanged(toIndex, target.slots[toIndex].Clone());
            }

            return changed;
        }

        /// <summary>
        /// Заменить содержимое слота валидированным стеком и уведомить слушателей.
        /// </summary>
        public void ReplaceSlot(int index, ItemStack newStack)
        {
            EnsureValidStack(newStack);
            EnsureIndexInRange(index);
            slots[index] = newStack.Clone();
            Events.InvokeSlotChanged(index, slots[index].Clone());
        }
        #endregion

        #region Очистка
        public virtual void Clear()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = ItemStack.Empty;
                Events.InvokeSlotChanged(i, ItemStack.Empty);
            }
        }
        #endregion
    }
}