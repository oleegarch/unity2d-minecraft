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
        public InventoryEvents Events { get; }

        /// <inheritdoc/>
        public int Capacity => slots.Length;

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
            if (stack == null || stack.IsEmpty || stack.Item == null || stack.Item.Id == 0)
                throw new InvalidDataException(nameof(stack));
        }
        #endregion

        #region Получение
        /// <summary>
        /// Возвращает копию конкретного слота по индексу.
        /// </summary>
        public ItemStack GetSlot(int index) => slots[EnsureIndexInRange(index)]?.Clone() ?? ItemStack.Empty;

        /// <summary>
        /// Возвращает поверхностную копию всех слотов (каждый ItemStack клонируется).
        /// </summary>
        public IReadOnlyList<ItemStack> GetAllSlots() => slots.Select(s => s?.Clone() ?? ItemStack.Empty).ToArray();

        /// <summary>
        /// Возвращает только непустые слоты (каждый ItemStack клонируется).
        /// </summary>
        public IReadOnlyList<ItemStack> GetNonEmptySlots() => slots.Where(s => !s.IsEmpty).Select(s => s.Clone()).ToArray();
        #endregion

        #region Проверки наличия
        /// <inheritdoc/>
        public bool HasEmptySlot
        {
            get
            {
                foreach (var slot in slots)
                    if (slot.IsEmpty) return true;

                return false;
            }
        }

        /// <inheritdoc/>
        public bool HasEmptySlots(int slotsCount) => GetEmptySlotsCount() >= slotsCount;

        /// Получить количество пустых слотов в инвентаре
        public int GetEmptySlotsCount()
        {
            int emptyCount = 0;
            foreach (var slot in slots)
                if (slot.IsEmpty) emptyCount++;

            return emptyCount;
        }

        /// <summary>
        /// Проверяет, что конкретный слот содержит тот же предмет и как минимум запрошенное количество.
        /// </summary>
        public virtual bool Has(ushort itemId, int needed, int slotIndex)
        {
            EnsureIndexInRange(slotIndex);

            var slot = slots[slotIndex];
            if (slot.IsEmpty || slot.Item == null) return false;

            return slot.Item.Id == itemId && slot.Quantity >= needed;
        }
        public virtual bool Has(ItemStack requested, int slotIndex)
        {
            EnsureValidStack(requested);

            return Has(requested.Item.Id, requested.Quantity, slotIndex);
        }

        /// <summary>
        /// Проверяет по всему инвентарю, хватает ли общего количества предмета.
        /// </summary>
        public virtual bool Has(ushort itemId, int needed)
        {
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
        public virtual bool Has(ItemStack requested)
        {
            EnsureValidStack(requested);

            int needed = requested.Quantity;
            var itemId = requested.Item.Id;

            return Has(itemId, needed);
        }
        #endregion

        #region Добавление
        /// <summary>
        /// Попытаться добавить стек: сначала заполняем существующие стеки того же типа, затем используем пустые слоты.
        /// ВАЖНО:
        /// Модифицирует стек переданный в аргументах!
        /// Если разместить всё не удалось — стек в аргументах будет содержать количество предметов которые остались.
        /// </summary>
        public virtual bool TryAdd(ItemStack stack)
        {
            EnsureValidStack(stack);

            // Заполняем существующие стеки
            for (int i = 0; i < slots.Length && stack.Quantity > 0; i++)
            {
                var slot = slots[i];
                if (!slot.IsEmpty && slot.CanStackWith(stack))
                {
                    int removed = stack.Remove(Math.Min(stack.Quantity, slot.MaxStack));
                    int added = slot.Add(removed);
                    if (added > 0) Events.InvokeSlotChanged(i, slots[i].Clone());
                }
            }

            // Используем пустые слоты
            for (int i = 0; i < slots.Length && stack.Quantity > 0; i++)
            {
                var slot = slots[i];
                if (slot.IsEmpty)
                {
                    slots[i] = stack.Clone();
                    stack.MakeEmpty();
                    Events.InvokeSlotChanged(i, slot.Clone());
                }
            }

            return stack.Quantity == 0;
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
        /// <summary>
        /// Пытается удалить указанный слот целиком. Возвращает true и отдаёт удалённый стек в out-параметре.
        /// </summary>
        public virtual bool Remove(int slotIndex, out ItemStack removed)
        {
            EnsureIndexInRange(slotIndex);
            removed = slots[slotIndex];

            var slot = slots[slotIndex];
            if (slot.IsEmpty) return false;

            slots[slotIndex] = ItemStack.Empty;
            Events.InvokeSlotChanged(slotIndex, slots[slotIndex].Clone());

            return true;
        }
        #endregion

        #region Забрать
        /// <summary>
        /// Пытается забрать <paramref name="stack.Quantity"/> предметов типа <paramref name="stack.Item"/> из инвентаря.
        /// Если предметов не хватает — ничего не заберёт и вернёт false.
        /// </summary>
        public virtual bool Take(ushort itemId, int needed)
        {
            // Сначала проверяем, что предметов хватает во всём инвентаре
            if (!Has(itemId, needed))
                return false;

            int removedQuantity = 0;

            for (int slotIndex = 0; slotIndex < slots.Length && needed > 0; slotIndex++)
            {
                var slot = slots[slotIndex];
                if (!slot.IsEmpty && slot.Item != null && slot.Item.Id == itemId)
                {
                    int toRemove = Math.Min(slot.Quantity, needed);
                    int removedCount = slot.Remove(toRemove);

                    if (removedCount > 0)
                    {
                        removedQuantity += removedCount;

                        if (slot.IsEmpty)
                            slots[slotIndex] = ItemStack.Empty;

                        Events.InvokeSlotChanged(slotIndex, slots[slotIndex].Clone());

                        needed -= removedCount;
                    }
                }
            }

            return true;
        }
        public bool Take(ItemStack stack)
        {
            EnsureValidStack(stack);

            int needed = stack.Quantity;
            var itemId = stack.Item.Id;

            return Take(itemId, needed);
        }
        #endregion

        #region Замена
        /// <summary>
        /// Заменить содержимое слота валидированным стеком и уведомить слушателей.
        /// </summary>
        public void Replace(int index, ItemStack newStack, out ItemStack old)
        {
            EnsureValidStack(newStack);
            EnsureIndexInRange(index);
            old = slots[index];
            slots[index] = newStack.Clone();
            Events.InvokeSlotChanged(index, slots[index].Clone());
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