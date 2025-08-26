using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace World.Inventories
{
    /// <summary>
    /// Универсальный drawer: рисует список слотов inventorySlotIndices (абсолютные индексы в Inventory).
    /// Рендерит слоты пачками (строками) через _uiRowSlotsPrefab.
    /// Количество слотов в одной строке задаётся _maxRowSlotsCount.
    /// Каждая следующая строка смещается вниз на (предыдущая_высота + _rowMarginBottom).
    /// </summary>
    public class UIInventorySlotsDrawer : MonoBehaviour, IDisposable
    {
        [SerializeField] protected GameObject _uiItemSlotPrefab;
        [SerializeField] protected GameObject _uiRowSlotsPrefab;
        [SerializeField] protected int _maxRowSlotsCount = 10;
        [SerializeField] protected int _rowMarginBottom = 8;

        protected Inventory _inventory;
        protected List<GameObject> _rowParents;
        protected UIItemSlotDrawer[] _uiItemSlots;
        protected int[] _inventoryIndices; // mapping: uiIndex -> inventoryIndex
        protected Dictionary<int, int> _invIndexToUiIndex; // mapping: inventoryIndex -> uiIndex
        protected bool _alwaysUpdate;

        public virtual void SetUp(Inventory inventory, IEnumerable<int> inventorySlotIndices, bool alwaysUpdate = false)
        {
            UnsubscribeFromInventoryEvents();
            Clear();

            // Валидация обязательных полей
            if (_uiRowSlotsPrefab == null) throw new InvalidOperationException("_uiRowSlotsPrefab must be assigned.");
            if (_uiItemSlotPrefab == null) throw new InvalidOperationException("_uiItemSlotPrefab must be assigned.");
            if (_maxRowSlotsCount <= 0) throw new InvalidOperationException("_maxRowSlotsCount must be > 0.");
            if (_rowMarginBottom < 0) _rowMarginBottom = 0; // допускаем 0, но не отрицательные

            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            _inventoryIndices = inventorySlotIndices.ToArray();
            if (_inventoryIndices.Length == 0) _inventoryIndices = Array.Empty<int>();

            int total = _inventoryIndices.Length;
            int rowsCount = (total + _maxRowSlotsCount - 1) / _maxRowSlotsCount;

            _uiItemSlots = new UIItemSlotDrawer[_inventoryIndices.Length];
            _invIndexToUiIndex = new Dictionary<int, int>(_inventoryIndices.Length);
            _alwaysUpdate = alwaysUpdate;

            // Создаём ровно rowsCount родителей-строк
            _rowParents = new List<GameObject>(rowsCount);
            for (int r = 0; r < rowsCount; r++)
            {
                GameObject rowGo = Instantiate(_uiRowSlotsPrefab, transform);
                rowGo.name = $"{name}_Row_{r}";
                _rowParents.Add(rowGo);
            }

            // Инстанцируем слоты и размещаем их в соответствующих строках
            for (int i = 0; i < _inventoryIndices.Length; i++)
            {
                int invIndex = _inventoryIndices[i];
                _invIndexToUiIndex[invIndex] = i;

                int rowIndex = i / _maxRowSlotsCount;
                Transform parentTransform = _rowParents[rowIndex].transform;

                GameObject go = Instantiate(_uiItemSlotPrefab, parentTransform);
                go.name = $"{name}_Slot_{i}";
                var uiSlot = go.GetComponent<UIItemSlotDrawer>();
                if (uiSlot == null) throw new InvalidOperationException("Prefab must have UIItemSlotDrawer.");

                // передаём стартовое состояние
                uiSlot.SetUp(DetermineSlotDirection(i), _inventory.GetSlot(invIndex));
                _uiItemSlots[i] = uiSlot;
            }

            // Обновляем layout'ы и корректируем позиции строк по высоте + отступ
            // Force UI rebuild чтобы rect.height был корректным (важно при использовании LayoutGroup/ContentSizeFitter)
            Canvas.ForceUpdateCanvases();
            for (int r = 0; r < _rowParents.Count; r++)
            {
                var rt = _rowParents[r].GetComponent<RectTransform>();
                if (rt == null) continue;
                LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            }

            // Ставим смещение для строк: row0 — оставляем на месте, последующие — под предыдущей с margin
            for (int r = 1; r < _rowParents.Count; r++)
            {
                var prevRt = _rowParents[r - 1].GetComponent<RectTransform>();
                var curRt = _rowParents[r].GetComponent<RectTransform>();
                if (prevRt == null || curRt == null) continue;

                // берём высоту предыдущей строки
                float prevHeight = prevRt.rect.height;

                // Важное замечание: anchoredPosition.y ведёт себя в зависимости от якорей.
                // Мы применяем смещение относительно текущей anchoredPosition.y предыдущей строки.
                float prevY = prevRt.anchoredPosition.y;
                float newCurY = prevY + (prevHeight + _rowMarginBottom);

                // сохраняем X
                Vector2 curAnch = curRt.anchoredPosition;
                curRt.anchoredPosition = new Vector2(curAnch.x, newCurY);
            }

            if (_alwaysUpdate) SubscribeToInventoryEvents();
        }

        /// <summary>Можно переопределить, чтобы задавать Left/Right/Center в специфичных реализациях (например для хотбара).</summary>
        protected virtual UIItemSlotDirection DetermineSlotDirection(int uiIndex)
        {
            // Защита от некорректных состояний
            if (_maxRowSlotsCount <= 0 || _inventoryIndices == null || _inventoryIndices.Length == 0)
                return UIItemSlotDirection.Center;

            int rowIndex = uiIndex / _maxRowSlotsCount;
            int posInRow = uiIndex % _maxRowSlotsCount;

            // Первый в ряду
            if (posInRow == 0)
                return UIItemSlotDirection.Left;

            // Вычисляем индекс последнего слота в этом ряду (учитываем неполную последнюю строку)
            int rowEndIndex = Math.Min((rowIndex + 1) * _maxRowSlotsCount - 1, _inventoryIndices.Length - 1);
            if (uiIndex == rowEndIndex)
                return UIItemSlotDirection.Right;

            return UIItemSlotDirection.Center;
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

            // удаляем созданные row parents
            if (_rowParents != null)
            {
                foreach (var row in _rowParents)
                {
                    if (row != null)
                        Destroy(row);
                }
                _rowParents = null;
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