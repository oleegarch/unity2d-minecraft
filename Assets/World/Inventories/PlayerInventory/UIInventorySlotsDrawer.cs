using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World.Inventories
{
    /// <summary>
    /// Универсальный drawer: рисует список слотов inventorySlotIndices (абсолютные индексы в Inventory).
    /// Рендерит слоты пачками (строками) через _uiRowSlotsPrefab.
    /// Количество слотов в одной строке задаётся _maxRowSlotsCount.
    /// </summary>
    public class UIInventorySlotsDrawer : MonoBehaviour, IDisposable
    {
        [SerializeField] protected GameObject _uiItemSlotPrefab;
        [SerializeField] protected GameObject _uiRowSlotsPrefab;
        [SerializeField] protected int _maxRowSlotsCount = 10;

        protected IInventory _inventory;
        protected List<GameObject> _rowParents;
        protected UIItemSlotDrawer[] _uiItemSlots;
        protected int[] _inventoryIndices; // mapping: uiIndex -> inventoryIndex
        protected Dictionary<int, int> _invIndexToUiIndex; // mapping: inventoryIndex -> uiIndex
        protected bool _alwaysUpdate;

        public virtual void SetUp(IInventory inventory, bool alwaysUpdate = false)
        {
            var indices = Enumerable.Range(0, inventory.SlotCount);
            SetUp(inventory, indices, alwaysUpdate: false);
        }
        public virtual void SetUp(IInventory inventory, IEnumerable<int> inventorySlotIndices, bool alwaysUpdate = false)
        {
            UnsubscribeFromInventoryEvents();
            Clear();

            // Валидация обязательных полей
            if (_uiRowSlotsPrefab == null) throw new InvalidOperationException("_uiRowSlotsPrefab must be assigned.");
            if (_uiItemSlotPrefab == null) throw new InvalidOperationException("_uiItemSlotPrefab must be assigned.");
            if (_maxRowSlotsCount <= 0) throw new InvalidOperationException("_maxRowSlotsCount must be > 0.");

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

            // Подготовим контейнеры для записи RectTransform'ов дочерних слотов по строкам,
            // чтобы позже можно было при необходимости инвертировать порядок дочерних элементов.
            var rowChildren = new List<List<RectTransform>>(rowsCount);
            for (int r = 0; r < rowsCount; r++) rowChildren.Add(new List<RectTransform>());

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

                // Запомним RectTransform дочернего слота для возможной перестановки порядка
                var childRt = go.GetComponent<RectTransform>();
                if (childRt != null) rowChildren[rowIndex].Add(childRt);

                // передаём стартовое состояние
                uiSlot.SetUp(DetermineSlotDirection(i), _inventory.GetSlot(invIndex));
                _uiItemSlots[i] = uiSlot;
            }

            // Ставим смещение для строк: row0 — оставляем на месте, последующие — под предыдущей
            for (int r = 1; r < _rowParents.Count; r++)
            {
                var prevRt = _rowParents[r - 1].GetComponent<RectTransform>();
                var curRt = _rowParents[r].GetComponent<RectTransform>();
                if (prevRt == null || curRt == null) continue;

                // anchoredPosition.y: смещаем в нужную сторону
                float newCurY = prevRt.anchoredPosition.y - prevRt.rect.height;
                curRt.anchoredPosition = new Vector2(curRt.anchoredPosition.x, newCurY);
            }

            if (_alwaysUpdate) SubscribeToInventoryEvents();
        }

        /// <summary>
        /// Возвращает флаговую комбинацию: тип (OneRow / MultiRow) + позиция по X (Left/XCenter/Right) + позиция по Y (Top/YCenter/Bottom).
        /// Учитывает _rowsPivot: при Bottom2Top визуальная "верхняя" строка — это та, у которой логический индекс rowsCount-1.
        /// </summary>
        protected virtual UIItemSlotDirection DetermineSlotDirection(int uiIndex)
        {
            // Защита на случай неверных входных данных
            if (_maxRowSlotsCount <= 0 || _inventoryIndices == null || _inventoryIndices.Length == 0)
                return UIItemSlotDirection.OneRow | UIItemSlotDirection.XCenter | UIItemSlotDirection.YCenter;

            int totalItems = _inventoryIndices.Length;
            if (uiIndex < 0 || uiIndex >= totalItems)
                return UIItemSlotDirection.OneRow | UIItemSlotDirection.XCenter | UIItemSlotDirection.YCenter;

            int rowsCount = (totalItems + _maxRowSlotsCount - 1) / _maxRowSlotsCount; // округление вверх
            int rowIndex = uiIndex / _maxRowSlotsCount;
            int posInRow = uiIndex % _maxRowSlotsCount;

            // Тип: OneRow если всего одна строка, иначе MultiRow
            UIItemSlotDirection type = (rowsCount == 1)
                ? UIItemSlotDirection.OneRow
                : UIItemSlotDirection.MultiRow;

            // Индексы начала/конца этой строки (учитываем неполную последнюю строку)
            int rowStartIndex = rowIndex * _maxRowSlotsCount;
            int rowEndIndex = Math.Min(rowStartIndex + _maxRowSlotsCount - 1, totalItems - 1);
            int rowLength = rowEndIndex - rowStartIndex + 1;

            // Горизонтальная позиция: Left / XCenter / Right
            UIItemSlotDirection horiz;
            if (rowLength == 1)
            {
                horiz = UIItemSlotDirection.XCenter;
            }
            else if (posInRow == 0)
            {
                horiz = UIItemSlotDirection.Left;
            }
            else if (uiIndex == rowEndIndex)
            {
                horiz = UIItemSlotDirection.Right;
            }
            else
            {
                horiz = UIItemSlotDirection.XCenter;
            }

            // Вертикальная позиция: Top / YCenter / Bottom в зависимости от логического положения строки от вершины
            UIItemSlotDirection vert;
            if (rowsCount == 1)
            {
                vert = UIItemSlotDirection.YCenter;
            }
            else if (rowIndex == 0)
            {
                vert = UIItemSlotDirection.Top;
            }
            else if (rowIndex == rowsCount - 1)
            {
                vert = UIItemSlotDirection.Bottom;
            }
            else
            {
                vert = UIItemSlotDirection.YCenter;
            }

            return type | horiz | vert;
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