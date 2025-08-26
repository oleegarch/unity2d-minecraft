// UIInventoryDrawer.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World.Inventories
{
    /// <summary>
    /// Универсальный drawer: рисует список слотов inventorySlotIndices (абсолютные индексы в Inventory).
    /// Может быть настроен на постоянный апдейт (alwaysUpdate) или подписываться только при Open().
    /// </summary>
    public class UIInventorySlotsDrawer : MonoBehaviour, IDisposable
    {
        [SerializeField] protected GameObject _uiItemSlotPrefab;

        protected Inventory _inventory;
        protected UIItemSlotDrawer[] _uiItemSlots;
        protected int[] _inventoryIndices; // mapping: uiIndex -> inventoryIndex
        protected Dictionary<int, int> _invIndexToUiIndex; // mapping: inventoryIndex -> uiIndex
        protected bool _alwaysUpdate;

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

            for (int i = 0; i < _inventoryIndices.Length; i++)
            {
                int invIndex = _inventoryIndices[i];
                _invIndexToUiIndex[invIndex] = i;

                GameObject go = Instantiate(_uiItemSlotPrefab, transform);
                var uiSlot = go.GetComponent<UIItemSlotDrawer>();
                if (uiSlot == null) throw new InvalidOperationException("Prefab must have UIItemSlotDrawer.");

                // передаём стартовое состояние
                uiSlot.SetUp(DetermineSlotDirection(i), _inventory.GetSlot(invIndex));
                _uiItemSlots[i] = uiSlot;
            }

            if (_alwaysUpdate) SubscribeToInventoryEvents();
        }

        /// <summary>Можно переопределить, чтобы задавать Left/Right/Center в специфичных реализациях (например для хотбара).</summary>
        protected virtual UIItemSlotDirection DetermineSlotDirection(int uiIndex) => UIItemSlotDirection.Center;

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
                _uiItemSlots[uiIndex].Refresh(newStack);
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
        private void OnInventorySlotChanged(int inventoryIndex, ItemStack newStack)
        {
            RefreshSlotByInventoryIndex(inventoryIndex, newStack);
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
        }
    }
}