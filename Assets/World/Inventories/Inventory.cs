using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using World.Items;

namespace World.Inventories
{
    public abstract class Inventory : IInventory
    {
        protected readonly ItemStack[] _slots;
        public int SlotCount => _slots.Length;
        public InventoryEvents Events { get; }

        protected Inventory(int slotCount)
        {
            if (slotCount <= 0) throw new ArgumentOutOfRangeException(nameof(slotCount));
            _slots = Enumerable.Repeat(ItemStack.Empty, slotCount).ToArray();
            Events = new InventoryEvents();
        }

        #region VALIDATORS
        private int ValidateIndex(int index)
        {
            if (
                index < 0 ||
                index >= _slots.Length
            )
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            return index;
        }
        private void ValidateStack(ItemStack stack)
        {
            if (
                stack == null ||
                stack.IsEmpty ||
                stack.Item.Sprite == null ||
                stack.Item.Id == 0 ||
                stack.Item.BlockId == 0
            )
            {
                throw new InvalidDataException(nameof(stack));
            }
        }
        #endregion

        #region GET OPERATIONS
        public IReadOnlyList<ItemStack> GetAllSlots() => _slots.Select(s => s?.Clone() ?? ItemStack.Empty).ToArray();
        public IReadOnlyList<ItemStack> GetAllNotEmptySlots() => _slots.Where(s => !s.IsEmpty).Select(s => s.Clone()).ToArray();
        public ItemStack GetSlot(int index) => _slots[ValidateIndex(index)]?.Clone() ?? ItemStack.Empty;

        // проверяет, что в указанном слоте лежит именно этот предмет (совпадает Id) и количество там ≥ нужного.
        public virtual bool Has(ItemStack stack, int slotIndex)
        {
            ValidateStack(stack);
            ValidateIndex(slotIndex);

            var slot = _slots[slotIndex];
            if (slot.IsEmpty || slot.Item == null)
                return false;

            return slot.Item.Id == stack.Item.Id && slot.Count >= stack.Count;
        }
        public bool Has(ItemInfo item, int slotIndex, int count = 1)
        {
            return Has(new ItemStack(item, count), slotIndex);
        }
        // проверяет, что во всём инвентаре есть хотя бы нужное количество предмета (суммируем по слотам).
        public virtual bool Has(ItemStack stack)
        {
            ValidateStack(stack);

            int needed = stack.Count;
            var itemId = stack.Item.Id;

            foreach (var slot in _slots)
            {
                if (!slot.IsEmpty && slot.Item != null && slot.Item.Id == itemId)
                {
                    needed -= slot.Count;
                    if (needed <= 0)
                        return true; // уже достаточно
                }
            }

            return false; // не хватило
        }
        public bool Has(ItemInfo item, int count = 1)
        {
            return Has(new ItemStack(item, count));
        }
        #endregion

        #region ADD OPERATION
        /// <summary>
        /// Попытаться добавить стек: сначала заполняем существующие стеки того же типа, затем пустые слоты.
        /// remainder - количество, которое не удалось разместить (0 если всё поместилось).
        /// Возвращаем true если весь стек помещён (remainder == 0).
        /// </summary>
        public virtual bool TryAdd(ItemStack stack, out int remainder)
        {
            ValidateStack(stack);

            int toPlace = stack.Count;
            var itemId = stack.Item.Id;

            // 1) заполнить существующие стеки
            for (int i = 0; i < _slots.Length && toPlace > 0; i++)
            {
                var s = _slots[i];
                if (!s.IsEmpty && s.Item != null && s.Item.Id == itemId)
                {
                    int added = s.Add(toPlace);
                    if (added > 0)
                        Events.InvokeSlotChanged(i, _slots[i].Clone());
                    toPlace -= added;
                }
            }

            // 2) использовать пустые слоты
            for (int i = 0; i < _slots.Length && toPlace > 0; i++)
            {
                var s = _slots[i];
                if (s.IsEmpty)
                {
                    int put = Math.Min(toPlace, stack.Item.MaxStack);
                    _slots[i] = new ItemStack(stack.Item, put);
                    Events.InvokeSlotChanged(i, _slots[i].Clone());
                    toPlace -= put;
                }
            }

            remainder = toPlace;
            return remainder == 0;
        }
        public virtual bool TryAdd(ItemInfo item, int count, out int remainder)
        {
            ItemStack stack = new ItemStack(item, count);
            return TryAdd(stack, out remainder);
        }
        #endregion

        #region REMOVE OPERATION
        /// <summary>
        /// Попытаться удалить amount из слота slotIndex. removed - фактически удалённый стек (или Empty).
        /// Возвращает true если удалено >0.
        /// </summary>
        public virtual bool TryRemove(int slotIndex, int amount, out ItemStack removed)
        {
            ValidateIndex(slotIndex);
            
            removed = ItemStack.Empty;
            if (amount <= 0) return false;

            var s = _slots[slotIndex];
            if (s.IsEmpty) return false;

            var item = s.Item; // может быть null если баг, но тогда Count >0 в идеале не бывает
            int removedCount = s.Remove(amount);

            if (removedCount > 0)
            {
                removed = new ItemStack(item, removedCount);
                // если стек стал пустым — убедимся, что он хранится как Empty
                if (_slots[slotIndex].IsEmpty)
                    _slots[slotIndex] = ItemStack.Empty;

                // уведомляем о новом состоянии слота
                Events.InvokeSlotChanged(slotIndex, _slots[slotIndex].Clone());
                return true;
            }

            return false;
        }
        #endregion

        #region MOVE OPERATIONS
        /// <summary>
        /// Переместить amount (или максимум) из fromIndex в toIndex.
        /// Поведение:
        /// - если to пустой -> перенос части/всего
        /// - если тот же тип -> попытка слить в стек (up to max stack)
        /// - если разные предметы -> swap
        /// Возвращает true если произошли изменения.
        /// </summary>
        public virtual bool Move(int fromIndex, int toIndex, int amount = int.MaxValue)
        {
            ValidateIndex(fromIndex);
            ValidateIndex(toIndex);
            if (fromIndex == toIndex) return false;

            var from = _slots[fromIndex];
            var to = _slots[toIndex];

            if (from.IsEmpty) return false;

            bool changed = false;

            // to пустой -> перенос
            if (to.IsEmpty)
            {
                int toMove = Math.Min(amount, from.Count);
                _slots[toIndex] = new ItemStack(from.Item, toMove);
                from.Remove(toMove);
                if (from.IsEmpty) _slots[fromIndex] = ItemStack.Empty;
                changed = true;
            }
            // тот же тип -> слить
            else if (!to.IsEmpty && !from.IsEmpty && to.Item != null && from.Item != null && to.Item.Id == from.Item.Id)
            {
                int canMove = Math.Min(amount, from.Count);
                int added = Math.Min(canMove, to.Item.MaxStack - to.Count);
                if (added > 0)
                {
                    to.Add(added);
                    from.Remove(added);
                    if (from.IsEmpty) _slots[fromIndex] = ItemStack.Empty;
                    changed = true;
                }
            }
            // разные предметы -> swap
            else
            {
                _slots[fromIndex] = to;
                _slots[toIndex] = from;
                changed = true;
            }

            if (changed)
            {
                Events.InvokeSlotChanged(fromIndex, _slots[fromIndex].Clone());
                Events.InvokeSlotChanged(toIndex, _slots[toIndex].Clone());
            }

            return changed;
        }

        /// <summary>
        /// Переместить amount (или максимум) из слота fromIndex этого инвентаря в слот toIndex другого инвентаря.
        /// Поведение:
        /// - если target == this — делегируется существующему Move.
        /// - если to пустой -> перенос части/всего (но не больше MaxStack этого предмета).
        /// - если тот же тип -> попытка слить в стек (up to max stack).
        /// - если разные предметы -> swap (полный обмен стеков между инвентарями).
        /// Возвращает true если произошли изменения.
        /// </summary>
        public virtual bool MoveTo(Inventory target, int fromIndex, int toIndex, int amount = int.MaxValue)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            ValidateIndex(fromIndex);
            target.ValidateIndex(toIndex);

            if (amount <= 0) return false;
            if (target == this) return Move(fromIndex, toIndex, amount);

            var from = _slots[fromIndex];
            var to = target._slots[toIndex];

            if (from.IsEmpty) return false;

            bool changed = false;

            // to пустой -> перенос (но не больше MaxStack)
            if (to.IsEmpty)
            {
                int toMove = Math.Min(amount, from.Count);
                var item = from.Item;
                if (item != null)
                    toMove = Math.Min(toMove, item.MaxStack);

                target._slots[toIndex] = new ItemStack(from.Item, toMove);
                from.Remove(toMove);
                if (from.IsEmpty) _slots[fromIndex] = ItemStack.Empty;
                changed = true;
            }
            // тот же тип -> слить (до max stack)
            else if (!to.IsEmpty && !from.IsEmpty && to.Item != null && from.Item != null && to.Item.Id == from.Item.Id)
            {
                int canMove = Math.Min(amount, from.Count);
                int space = to.Item.MaxStack - to.Count;
                int added = Math.Min(canMove, space);
                if (added > 0)
                {
                    to.Add(added);
                    from.Remove(added);
                    if (from.IsEmpty) _slots[fromIndex] = ItemStack.Empty;
                    changed = true;
                }
            }
            // разные предметы -> swap (полный обмен стеков)
            else
            {
                _slots[fromIndex] = to;
                target._slots[toIndex] = from;
                changed = true;
            }

            if (changed)
            {
                Events.InvokeSlotChanged(fromIndex, _slots[fromIndex].Clone());
                target.Events.InvokeSlotChanged(toIndex, target._slots[toIndex].Clone());
            }

            return changed;
        }
        public void ReplaceSlot(int index, ItemStack newStack)
        {
            ValidateStack(newStack);
            ValidateIndex(index);
            _slots[index] = newStack;
            Events.InvokeSlotChanged(index, _slots[index].Clone());
        }
        #endregion

        #region CLEAR/DISPOSE
        public virtual void Clear()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = ItemStack.Empty;
                Events.InvokeSlotChanged(i, ItemStack.Empty);
            }
        }
        #endregion
    }
}