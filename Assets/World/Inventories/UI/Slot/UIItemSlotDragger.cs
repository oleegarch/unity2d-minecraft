using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using World.UI;

namespace World.Inventories
{
    #region Контекст и тип слота
    public enum SlotType
    {
        // Обычный слот любого инвентаря. Стандартное поведение с перетаскиванием.
        Default,
        // Креативный слот (без инвентаря). Поведение клонирования элементов.
        Creative,
        // Превью слот (без инвентаря). Нужен для отображения без функционала перетаскивания.
        Preview
    }

    public class SlotContext
    {
        public Inventory Inventory { get; private set; }
        public int SlotIndex { get; private set; }
        public SlotType SlotType { get; private set; }

        private ItemStack _predefinedItemStack;
        public ItemStack ItemStack
        {
            get
            {
                if (_predefinedItemStack != null) return _predefinedItemStack;
                return Inventory.GetSlot(SlotIndex);
            }
            set
            {
                _predefinedItemStack = value;
            }
        }

        public SlotContext(Inventory inventory, int slotIndex, SlotType slotType = SlotType.Default)
        {
            Inventory = inventory;
            SlotIndex = slotIndex;
            SlotType = slotType;
        }
        public SlotContext(ItemStack stack, SlotType slotType)
        {
            ItemStack = stack;
            SlotType = slotType;
        }
    }
    #endregion

    public class UIItemSlotDragger : MonoBehaviour, IPointerClickHandler, IDropHandler
    {
        #region Поля
        public static bool DraggingByClick;
        public static bool DraggingStarted => _draggingStartedFromDragger != null;
        private static int _draggingQuantity;
        private static GameObject _draggingStackGO;
        private static UIItemSlotDragger _draggingStartedFromDragger;
        private static RectTransform _draggingStackRT => _draggingStackGO.GetComponent<RectTransform>();
        private static UIImageWithLabel _draggingStackDrawer => _draggingStackGO.GetComponent<UIImageWithLabel>();

        [SerializeField] private UIItemSlotDragHandlers _dragHandlers;
        [SerializeField] private UIImageWithLabel _sourceStackDrawer;
        [SerializeField] private GameObject _draggingStackPrefab;

        private UIItemSlotDrawer _currentDrawer;
        private Canvas _parentCanvas;
        private RectTransform _parentCanvasRT => _parentCanvas.transform as RectTransform;
        private RectTransform _rectTransform;
        private SlotContext _currentSlotContext;
        private bool _dragging = true;
        private bool _dragHandlersEnabled = true;
        private bool _dropEnabled = true;
        private bool _modifyingSourceStack = true;

        public bool DraggingDisabled => !_dragging;
        public SlotContext CurrentSlotContext => _currentSlotContext;

        public event Action<UIItemSlotDrawer, UIItemSlotDragger> OnClick; // при обычном клике на сам слот
        public event Action<UIItemSlotDragger, UIItemSlotDragger> OnBeforeDrop; // событие вызывающееся "до попытки переместить ТЕКУЩИЙ слот в другой слот"
        public event Action<UIItemSlotDragger, UIItemSlotDragger> OnDropped; // событие вызывающееся "после перемещения ТЕКУЩЕГО слота в другой слот"
        #endregion

        #region Жизненный цикл
        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            _parentCanvas = GetComponentInParent<Canvas>();
            _currentDrawer = GetComponent<UIItemSlotDrawer>();
        }
        private void Update()
        {
            if (DraggingByClick && _draggingStartedFromDragger == this)
            {
                ContinueDrag(Mouse.current.position.ReadValue());
            }
        }
        private void OnDestroy()
        {
            if (_draggingStartedFromDragger == this)
            {
                DestroyDraggingStack();
            }
        }
        private void OnEnable()
        {
            _dragHandlers.StartDrag += StartDrag;
            _dragHandlers.ContinueDrag += ContinueDrag;
            _dragHandlers.FinishDrag += FinishDrag;
        }
        private void OnDisable()
        {
            _dragHandlers.StartDrag -= StartDrag;
            _dragHandlers.ContinueDrag -= ContinueDrag;
            _dragHandlers.FinishDrag -= FinishDrag;
        }
        #endregion

        #region События Drop
        public void OnDrop(PointerEventData eventData)
        {
            if (DraggingByClick || !_dragging || !_dropEnabled) return;
            ProcessDrop(eventData.pointerDrag?.GetComponent<UIItemSlotDragger>(), _draggingQuantity);
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(_currentDrawer, this);

            if (_dragging == false) return;

            int dropCount = _draggingQuantity;
            bool isRightClick = eventData.button == PointerEventData.InputButton.Right;
            bool continueDragging = false;
            bool oldDraggingByClick = DraggingByClick;

            if (!DraggingByClick)
            {
                int draggingQuantity = CurrentSlotContext.ItemStack.Quantity;
                if (isRightClick)
                    draggingQuantity = Math.Max(1, draggingQuantity / 2);

                if (StartDrag(eventData.position, draggingQuantity))
                {
                    _dragHandlers.enabled = false;
                    DraggingByClick = true;
                }
            }
            else if (isRightClick)
            {
                dropCount = 1;
                _draggingQuantity -= dropCount;
                continueDragging = _draggingQuantity > 0;
            }

            if (_draggingStackGO != null && oldDraggingByClick && _dropEnabled)
            {
                ProcessDrop(_draggingStartedFromDragger, dropCount);

                if (continueDragging)
                {
                    _draggingStartedFromDragger.UpdateDraggingStack(_draggingQuantity);
                    _draggingStartedFromDragger.ModifySourceStackDrawer();
                }
                else
                {
                    _draggingStartedFromDragger._dragHandlers.enabled = _draggingStartedFromDragger._dragHandlersEnabled;
                    _draggingStartedFromDragger.FinishDrag();
                }
            }
        }
        #endregion

        #region События Drag
        /// <summary>Начать перетаскивание (создаёт статический визуал и скрывает оригинал).</summary>
        public bool StartDrag(Vector3 position, int draggingQuantity)
        {
            bool started = _draggingStackGO == null && _currentDrawer.HasContent;
            if (started)
            {
                SetUpDraggingStack(draggingQuantity);
                ModifySourceStackDrawer();
                UpdateDraggingStackPosition(position);
            }
            return started;
        }
        public void StartDrag(Vector3 position)
        {
            StartDrag(position, CurrentSlotContext.ItemStack.Quantity);
        }

        /// <summary>Обновить позицию перетаскиваемого визуала.</summary>
        public void ContinueDrag(Vector3 position)
        {
            UpdateDraggingStackPosition(position);
        }

        /// <summary>Завершить перетаскивание (удаляет статик и показывает оригинал).</summary>
        public void FinishDrag()
        {
            ShowSourceStackDrawer();
            DestroyDraggingStack();
        }

        /// <summary>Обработать Drop: логика перемещения/клонирования/удаления стака.</summary>
        public void ProcessDrop(UIItemSlotDragger fromDragger, int quantity)
        {
            fromDragger.OnBeforeDrop?.Invoke(fromDragger, this);

            SlotContext fromContext = fromDragger.CurrentSlotContext;
            SlotContext toContext = this.CurrentSlotContext;

            if (fromContext == null || toContext == null || fromDragger.DraggingDisabled) return;

            // При перемещении из обычного слота в креативный слот — УДАЛЕНИЕ
            if (fromContext.SlotType == SlotType.Default && toContext.SlotType == SlotType.Creative)
            {
                ItemStack replaceItemStack = fromContext.ItemStack.Clone();
                replaceItemStack.Remove(quantity);
                fromContext.Inventory.Replace(fromContext.SlotIndex, replaceItemStack, out ItemStack old);
            }
            // При перемещении из креативного слота в обычный слот — КЛОНИРОВАНИЕ
            else if (fromContext.SlotType == SlotType.Creative && toContext.SlotType == SlotType.Default)
            {
                ItemStack replaceItemStack = fromContext.ItemStack.Clone();
                replaceItemStack.SetQuantity(quantity);
                toContext.Inventory.Replace(toContext.SlotIndex, replaceItemStack, out ItemStack old);
            }
            // При перемещении из обычного слота в обычный слот — ПЕРЕМЕЩЕНИЕ
            else if (fromContext.SlotType == SlotType.Default && toContext.SlotType == SlotType.Default)
            {
                fromContext.Inventory.MoveTo(toContext.Inventory, fromContext.SlotIndex, toContext.SlotIndex, quantity);
            }

            fromDragger.OnDropped?.Invoke(fromDragger, this);
        }
        #endregion

        #region Контекст слота
        public void SetSlotContextCustom(SlotContext context)
        {
            _currentSlotContext = context;
        }
        public void SetSlotContext(SlotContext context)
        {
            _currentSlotContext = context;

            ToggleModifyingSourceStack(context.SlotType == SlotType.Default);
            ToggleDragHandlers(context.SlotType == SlotType.Default);
            ToggleDragging(context.SlotType == SlotType.Default || context.SlotType == SlotType.Creative);
            ToggleDrop(context.SlotType == SlotType.Default || context.SlotType == SlotType.Creative);
        }
        public void ToggleDragHandlers(bool enabled)
        {
            _dragHandlers.enabled = enabled;
            _dragHandlersEnabled = enabled;
        }
        public void ToggleDragging(bool enabled)
        {
            _dragging = enabled;
        }
        public void ToggleDrop(bool enabled)
        {
            _dropEnabled = enabled;
        }
        #endregion

        #region Перемещаемый стек
        public void UpdateDraggingStack(Sprite sprite, int count)
        {
            if (_draggingStartedFromDragger == this)
            {
                _draggingStackDrawer.SetUp(sprite, count.ToString());
                _draggingQuantity = count;
            }
        }
        public void UpdateDraggingStack(int count)
        {
            if (_draggingStartedFromDragger == this)
            {
                _draggingStackDrawer.SetLabelText(count.ToString());
                _draggingQuantity = count;
            }
        }
        private void SetUpDraggingStack(int draggingQuantity)
        {
            DestroyDraggingStack();

            _draggingQuantity = draggingQuantity;
            _draggingStartedFromDragger = this;
            _draggingStackGO = Instantiate(_draggingStackPrefab, _rectTransform.position, Quaternion.identity, _parentCanvas.transform);
            _draggingStackDrawer.SetUp(_currentDrawer.StackDrawer.GetCurrentImageSprite(), _draggingQuantity.ToString());
        }
        private void UpdateDraggingStackPosition(Vector3 position)
        {
            if (_draggingStackGO == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvasRT,
                position,
                _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera,
                out var localPos))
            {
                _draggingStackRT.localPosition = localPos;
            }
        }
        private void DestroyDraggingStack()
        {
            if (_draggingStackGO != null)
            {
                Destroy(_draggingStackGO);
                _draggingStartedFromDragger = null;
                _draggingStackGO = null;
                DraggingByClick = false;
            }
        }

        #endregion

        #region Исходный стек
        public void ToggleModifyingSourceStack(bool enabled)
        {
            _modifyingSourceStack = enabled;
        }
        private void ModifySourceStackDrawer()
        {
            if (_modifyingSourceStack == true)
            {
                int left = CurrentSlotContext.ItemStack.Quantity - _draggingQuantity;
                _sourceStackDrawer.SetLabelText(left.ToString());
                _sourceStackDrawer.gameObject.SetActive(left > 0);
            }
        }
        private void ShowSourceStackDrawer()
        {
            if (_modifyingSourceStack == true)
            {
                if (!CurrentSlotContext.ItemStack.IsEmpty)
                {
                    _sourceStackDrawer.SetLabelText(CurrentSlotContext.ItemStack.Quantity.ToString());
                }
                _sourceStackDrawer.gameObject.SetActive(true);
            }
        }
        #endregion
    }
}