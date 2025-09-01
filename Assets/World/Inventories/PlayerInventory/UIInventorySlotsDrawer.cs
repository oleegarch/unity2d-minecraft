using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Chunks;

namespace World.Inventories
{
    /// <summary>
    /// Универсальный drawer: рисует список слотов inventorySlotIndices (абсолютные индексы в Inventory).
    /// Рендерит слоты пачками (строками) через _uiRowSlotsPrefab.
    /// Количество слотов в одной строке задаётся _maxRowSlotsCount.
    /// </summary>
    public class UIInventorySlotsDrawer : MonoBehaviour, IDisposable
    {
        [SerializeField] protected Transform _uiSlotsParent;
        [SerializeField] protected GameObject _uiItemSlotPrefab;
        [SerializeField] protected WorldManager _manager;

        protected Inventory _inventory;
        protected UIItemSlotDrawer[] _uiItemSlots;
        protected int[] _inventoryIndices; // mapping: uiIndex -> inventoryIndex
        protected Dictionary<int, int> _invIndexToUiIndex; // mapping: inventoryIndex -> uiIndex
        protected bool _alwaysUpdate;

        public virtual void SetUp(WorldManager manager, Inventory inventory, bool alwaysUpdate = false)
        {
            _manager = manager;
            SetUp(inventory, alwaysUpdate);
        }
        public virtual void SetUp(Inventory inventory, bool alwaysUpdate = false)
        {
            var indices = Enumerable.Range(0, inventory.Capacity);
            SetUp(inventory, indices, alwaysUpdate: false);
        }
        public virtual void SetUp(Inventory inventory, IEnumerable<int> inventorySlotIndices, bool alwaysUpdate = false)
        {
            UnsubscribeFromInventoryEvents();
            Clear();

            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _inventoryIndices = inventorySlotIndices.ToArray();
            if (_inventoryIndices.Length == 0) _inventoryIndices = Array.Empty<int>();

            _uiItemSlots = new UIItemSlotDrawer[_inventoryIndices.Length];
            _invIndexToUiIndex = new Dictionary<int, int>(_inventoryIndices.Length);
            _alwaysUpdate = alwaysUpdate;

            // Инстанцируем слоты и размещаем их в соответствующих строках
            for (int i = 0; i < _inventoryIndices.Length; i++)
            {
                int invIndex = _inventoryIndices[i];
                _invIndexToUiIndex[invIndex] = i;

                GameObject go = Instantiate(_uiItemSlotPrefab, _uiSlotsParent);
                go.name = $"{name}_Slot_{i}";
                var uiSlot = go.GetComponent<UIItemSlotDrawer>();
                if (uiSlot == null) throw new InvalidOperationException("Prefab must have UIItemSlotDrawer.");

                // передаём стартовое состояние
                uiSlot.SetUp(_inventory, invIndex, _manager.ItemDatabase);
                _uiItemSlots[i] = uiSlot;
            }

            if (_alwaysUpdate) SubscribeToInventoryEvents();
        }

        public virtual void Open()
        {
            gameObject.SetActive(true);
            SubscribeToInventoryEvents();
            RefreshAll();
        }
        public virtual void Close()
        {
            UnsubscribeFromInventoryEvents();
            gameObject.SetActive(false);
        }
        public virtual bool Toggle()
        {
            bool isActive = gameObject.activeInHierarchy;

            if (isActive)
                Close();
            else
                Open();

            return !isActive;
        }

        protected virtual void RefreshAll()
        {
            if (_uiItemSlots == null || _inventory == null) return;
            for (int i = 0; i < _uiItemSlots.Length; i++)
            {
                int invIndex = _inventoryIndices[i];
                _uiItemSlots[i].Refresh(_inventory.GetSlot(invIndex));
            }
        }

        /// <summary>Обновление слота по глобальному индексу (тот, который шлёт Inventory.Events).</summary>
        protected virtual void RefreshSlotByInventoryIndex(int inventoryIndex, ItemStack newStack)
        {
            if (_invIndexToUiIndex == null) return;
            if (_invIndexToUiIndex.TryGetValue(inventoryIndex, out int uiIndex))
                // используем состояние из Inventory чтобы быть уверенными в консистентности
                _uiItemSlots[uiIndex].Refresh(_inventory.GetSlot(inventoryIndex));
        }

        #region Events
        protected void SubscribeToInventoryEvents()
        {
            if (_inventory == null) return;
            _inventory.Events.SlotChanged += OnInventorySlotChanged;
        }
        protected void UnsubscribeFromInventoryEvents()
        {
            if (_inventory == null) return;
            _inventory.Events.SlotChanged -= OnInventorySlotChanged;
        }
        private void OnInventorySlotChanged(object sender, SlotChangedEventArgs args)
        {
            RefreshSlotByInventoryIndex(args.SlotIndex, args.NewValue);
        }
        #endregion

        public virtual void Clear()
        {
            if (_uiItemSlots != null)
            {
                foreach (var s in _uiItemSlots) s?.Dispose();
                _uiItemSlots = null;
            }

            _inventory = null;
            _inventoryIndices = null;
            _invIndexToUiIndex = null;
        }

        public virtual void Dispose()
        {
            UnsubscribeFromInventoryEvents();
            Clear();
            if (gameObject != null)
                Destroy(gameObject);
        }
    }
}