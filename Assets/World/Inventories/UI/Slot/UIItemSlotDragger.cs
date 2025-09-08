using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using World.UI;

namespace World.Inventories
{
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

    public class UIItemSlotDragger : MonoBehaviour, IPointerClickHandler, IDropHandler
    {
        public static bool DraggingByClick;
        public static bool DraggingStarted => _draggingSlotStartedFromDragger != null;
        private static GameObject _draggingStackGO;
        private static UIItemSlotDragger _draggingSlotStartedFromDragger;
        private static RectTransform _draggingStackRT => _draggingStackGO.GetComponent<RectTransform>();
        private static UIImageWithLabel _draggingStackDrawer => _draggingStackGO.GetComponent<UIImageWithLabel>();
        private static SlotContext _draggingSlotContext => _draggingSlotStartedFromDragger.CurrentSlotContext;
        private static ItemStack _draggingStack => _draggingSlotContext.ItemStack;

        [SerializeField] private UIItemSlotDragHandlers _dragHandlers;
        [SerializeField] private GameObject _draggingStackPrefab;
        [SerializeField] private GameObject _currentItemStackGameObject;

        private UIItemSlotDrawer _currentDrawer;
        private Canvas _parentCanvas;
        private RectTransform _parentCanvasRT => _parentCanvas.transform as RectTransform;
        private RectTransform _rectTransform;
        private SlotContext _currentSlotContext;
        private bool _currentItemStackActive = true;
        private bool _dragging = true;
        private bool _dragHandlersEnabled = true;
        private bool _dropEnabled = true;
        private bool _hidingSourceStack = true;

        public bool DraggingDisabled => !_dragging;
        public SlotContext CurrentSlotContext => _currentSlotContext;

        public event Action<UIItemSlotDrawer, UIItemSlotDragger> OnClick; // при обычном клике на сам слот
        public event Action<UIItemSlotDragger, UIItemSlotDragger> OnBeforeDrop; // событие вызывающееся "до попытки переместить ТЕКУЩИЙ слот в другой слот"
        public event Action<UIItemSlotDragger, UIItemSlotDragger> OnDropped; // событие вызывающееся "после перемещения ТЕКУЩЕГО слота в другой слот"

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            _parentCanvas = GetComponentInParent<Canvas>();
            _currentDrawer = GetComponent<UIItemSlotDrawer>();
        }
        private void Update()
        {
            if (DraggingByClick && _draggingSlotStartedFromDragger == this)
            {
                ContinueDrag(Mouse.current.position.ReadValue());
            }
        }
        private void OnDestroy()
        {
            if (_draggingSlotStartedFromDragger == this)
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

        #region Pointer handlers
        public void OnDrop(PointerEventData eventData)
        {
            if (DraggingByClick || !_dragging || !_dropEnabled) return;
            ProcessDrop(eventData.pointerDrag?.GetComponent<UIItemSlotDragger>());
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(_currentDrawer, this);

            if (_dragging == false) return;

            if (!DraggingByClick)
            {
                StartDrag(eventData.position);

                if (_draggingStackGO != null)
                {
                    _dragHandlers.enabled = false;
                    DraggingByClick = true;
                }
            }
            else if (_draggingStackGO != null && _dropEnabled)
            {
                ProcessDrop(_draggingSlotStartedFromDragger);
                _draggingSlotStartedFromDragger.FinishDrag();
            }
        }
        #endregion

        #region Drag'n Drop
        /// <summary>Начать перетаскивание (создаёт статический визуал и скрывает оригинал).</summary>
        public void StartDrag(Vector3 position)
        {
            if (_currentDrawer.HasContent)
            {
                DestroyDraggingStack();
                ToggleSourceStackDrawer(false);
                SetUpDraggingStack();
                UpdateDraggingStackPosition(position);
            }
        }

        /// <summary>Обновить позицию перетаскиваемого визуала.</summary>
        public void ContinueDrag(Vector3 position)
        {
            UpdateDraggingStackPosition(position);
        }

        /// <summary>Завершить перетаскивание (удаляет статик и показывает оригинал).</summary>
        public void FinishDrag()
        {
            DestroyDraggingStack();
            ToggleSourceStackDrawer(true);
        }

        /// <summary>Обработать Drop: логика перемещения/клонирования/удаления стака.</summary>
        public void ProcessDrop(UIItemSlotDragger fromDragger)
        {
            fromDragger.OnBeforeDrop?.Invoke(fromDragger, this);

            SlotContext fromContext = fromDragger.CurrentSlotContext;
            SlotContext toContext = this.CurrentSlotContext;

            if (fromContext == null || toContext == null || fromDragger.DraggingDisabled) return;

            // При перемещении из обычного слота в креативный слот — УДАЛЕНИЕ
            if (fromContext.SlotType == SlotType.Default && toContext.SlotType == SlotType.Creative)
            {
                fromContext.Inventory.Remove(fromContext.SlotIndex, out ItemStack removed);
            }
            // При перемещении из креативного слота в обычный слот — КЛОНИРОВАНИЕ целого стака
            else if (fromContext.SlotType == SlotType.Creative && toContext.SlotType == SlotType.Default)
            {
                toContext.Inventory.Replace(toContext.SlotIndex, fromContext.ItemStack, out ItemStack old);
            }
            // При перемещении из обычного слота в обычный слот — ПЕРЕМЕЩЕНИЕ
            else if (fromContext.SlotType == SlotType.Default && toContext.SlotType == SlotType.Default)
            {
                fromContext.Inventory.MoveTo(toContext.Inventory, fromContext.SlotIndex, toContext.SlotIndex, _draggingStack.Quantity);
            }

            fromDragger._dragHandlers.enabled = _dragHandlersEnabled;
            fromDragger.OnDropped?.Invoke(fromDragger, this);
        }
        #endregion

        #region Хелперы
        public void SetSlotContextCustom(SlotContext context)
        {
            _currentSlotContext = context;
        }
        public void SetSlotContext(SlotContext context)
        {
            _currentSlotContext = context;

            ToggleHidingSourceStack(context.SlotType == SlotType.Default);
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
        public void ToggleHidingSourceStack(bool enabled)
        {
            _hidingSourceStack = enabled;
            _currentItemStackGameObject.SetActive(!_hidingSourceStack || _currentItemStackActive);
        }

        public void UpdateDraggingStackDrawer(Sprite sprite, int count)
        {
            if (_draggingSlotStartedFromDragger == this)
            {
                _draggingStackDrawer.SetUp(sprite, count.ToString());
            }
        }
        private void SetUpDraggingStack()
        {
            if (_draggingStackGO == null)
            {
                _draggingSlotStartedFromDragger = this;
                _draggingStackGO = Instantiate(_draggingStackPrefab, _rectTransform.position, Quaternion.identity, _parentCanvas.transform);
            }

            if (_draggingSlotStartedFromDragger == this)
            {
                _draggingStackDrawer.SetUp(_currentDrawer.StackDrawer);
            }
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

        private void ToggleSourceStackDrawer(bool active)
        {
            if (_currentItemStackGameObject != null && _hidingSourceStack == true)
            {
                _currentItemStackActive = active;
                _currentItemStackGameObject.SetActive(_currentItemStackActive);
            }
        }

        private void DestroyDraggingStack()
        {
            if (_draggingStackGO != null)
            {
                Destroy(_draggingStackGO);
                _draggingSlotStartedFromDragger = null;
                _draggingStackGO = null;
                DraggingByClick = false;
            }
        }
        #endregion
    }
}