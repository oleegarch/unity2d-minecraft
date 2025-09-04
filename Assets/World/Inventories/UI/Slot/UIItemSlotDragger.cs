using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using World.Items;
using World.UI;

namespace World.Inventories
{
    public enum SlotType
    {
        Default,
        Creative,
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

        public SlotContext(Inventory inventory, int slotIndex)
        {
            Inventory = inventory;
            SlotIndex = slotIndex;
            SlotType = SlotType.Default;
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
        public static GameObject DraggingStackGO;
        public static UIItemSlotDragger DraggingSlotStartedFromDragger;
        private static RectTransform _draggingStackRT => DraggingStackGO.GetComponent<RectTransform>();
        private static UIImageWithLabel _draggingStackDrawer => DraggingStackGO.GetComponent<UIImageWithLabel>();
        private static SlotContext _draggingSlotContext => DraggingSlotStartedFromDragger.CurrentSlotContext;
        private static ItemStack _draggingStack => _draggingSlotContext.ItemStack;

        [SerializeField] private UIItemSlotDragHandlers _dragHandlers;
        [SerializeField] private GameObject _draggingStackPrefab;
        [SerializeField] private GameObject _currentItemStackGameObject;
        private bool _currentItemStackActive = true;

        private UIItemSlotDrawer _currentDrawer;
        private Canvas _parentCanvas;
        private RectTransform _parentCanvasRT => _parentCanvas.transform as RectTransform;
        private RectTransform _rectTransform;
        private SlotContext _currentSlotContext;
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
            if (DraggingByClick && DraggingSlotStartedFromDragger == this)
            {
                ContinueDrag(Mouse.current.position.ReadValue());
            }
        }
        private void OnDestroy()
        {
            if (DraggingSlotStartedFromDragger == this)
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
                
                if (DraggingStackGO != null)
                {
                    _dragHandlers.enabled = false;
                    DraggingByClick = true;
                }
            }
            else if(DraggingStackGO != null && _dropEnabled)
            {
                ProcessDrop(DraggingSlotStartedFromDragger);
                DraggingSlotStartedFromDragger.FinishDrag();
            }
        }
        #endregion

        #region Drag'n Drop
        /// <summary>Начать перетаскивание (создаёт статический визуал и скрывает оригинал).</summary>
        public void StartDrag(Vector3 position)
        {
            if (CurrentSlotContext?.ItemStack != null && !CurrentSlotContext.ItemStack.IsEmpty)
            {
                DestroyDraggingStack();
                ToggleCurrentStackDrawer(false);

                DraggingSlotStartedFromDragger = this;
                DraggingStackGO = Instantiate(_draggingStackPrefab, _rectTransform.position, Quaternion.identity, _parentCanvas.transform);
                _draggingStackDrawer.SetUp(_currentDrawer.StackDrawer);

                UpdateDraggedPosition(position);
            }
        }

        /// <summary>Обновить позицию перетаскиваемого визуала.</summary>
        public void ContinueDrag(Vector3 position)
        {
            UpdateDraggedPosition(position);
        }

        /// <summary>Завершить перетаскивание (удаляет статик и показывает оригинал).</summary>
        public void FinishDrag()
        {
            DestroyDraggingStack();
            ToggleCurrentStackDrawer(true);
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

            _dragHandlers.enabled = _dragHandlersEnabled;

            fromDragger.OnDropped?.Invoke(fromDragger, this);
        }
        #endregion

        #region Хелперы
        public void SetSlotContext(SlotContext context)
        {
            _currentSlotContext = context;

            if (DraggingSlotStartedFromDragger == this)
                _draggingStackDrawer.SetUp(_currentDrawer.StackDrawer);
        }
        public void DisableDragHandlers()
        {
            _dragHandlers.enabled = false;
            _dragHandlersEnabled = false;
        }
        public void DisableDragging()
        {
            DisableDragHandlers();
            _dragging = false;
        }
        public void DisableDrop()
        {
            _dropEnabled = false;
        }
        public void ToggleHidingSourceStack(bool enabled)
        {
            _hidingSourceStack = enabled;
            _currentItemStackGameObject.SetActive(_currentItemStackActive);
        }

        private void UpdateDraggedPosition(Vector3 position)
        {
            if (DraggingStackGO == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvasRT,
                position,
                _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera,
                out var localPos))
            {
                _draggingStackRT.localPosition = localPos;
            }
        }

        private void ToggleCurrentStackDrawer(bool active)
        {
            _currentItemStackActive = active;
            if (_currentItemStackGameObject != null && _hidingSourceStack == true)
                _currentItemStackGameObject.SetActive(_currentItemStackActive);
        }

        private void DestroyDraggingStack()
        {
            if (DraggingStackGO != null)
            {
                Destroy(DraggingStackGO);
                DraggingSlotStartedFromDragger = null;
                DraggingStackGO = null;
                DraggingByClick = false;
            }
        }
        #endregion
    }
}