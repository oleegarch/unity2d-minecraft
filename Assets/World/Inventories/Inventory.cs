using System;
using System.Collections.Generic;
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

        public IReadOnlyList<ItemStack> GetAllSlots()
        {
            return _slots.Select(s => s?.Clone() ?? ItemStack.Empty).ToArray();
        }

        public ItemStack GetSlot(int index)
        {
            ValidateIndex(index);
            return _slots[index]?.Clone() ?? ItemStack.Empty;
        }
        public void ReplaceSlot(int index, ItemStack newStack)
        {
            ValidateIndex(index);
            _slots[index] = newStack;
            Events.InvokeSlotChanged(index, _slots[index].Clone());
        }

        private void ValidateIndex(int index)
        {
            if (index < 0 || index >= _slots.Length) throw new ArgumentOutOfRangeException(nameof(index));
        }

        /// <summary>
        /// Попытаться добавить стек: сначала заполняем существующие стеки того же типа, затем пустые слоты.
        /// remainder - количество, которое не удалось разместить (0 если всё поместилось).
        /// Возвращаем true если весь стек помещён (remainder == 0).
        /// </summary>
        public virtual bool TryAdd(ItemStack stack, out int remainder)
        {
            if (stack == null || stack.IsEmpty || stack.Item == null || stack.Count <= 0)
            {
                remainder = 0;
                return true;
            }

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

        public virtual bool TryAdd(ItemInfo item)
        {
            ItemStack stack = new ItemStack(item);
            return TryAdd(stack, out int remainder);
        }

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

        public virtual void Clear()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = ItemStack.Empty;
                Events.InvokeSlotChanged(i, ItemStack.Empty);
            }
        }
    }
}