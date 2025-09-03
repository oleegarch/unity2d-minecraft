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
        public ItemStack ItemStack { get; private set; }
        public SlotType SlotType { get; private set; }

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
        private static UIItemSlotDragger _draggingStartedByClickFromSlotDragger;
        private static GameObject _draggingStackGO;
        private static RectTransform _draggingStackRT;
        private static UIImageWithLabel _draggingStackDrawer;
        public static bool DraggingByClick => _draggingStartedByClickFromSlotDragger != null;
        public static ItemStack DraggingStack;

        [SerializeField] private UIItemSlotDragHandlers _dragHandlers;
        [SerializeField] private GameObject _draggingStackPrefab;
        [SerializeField] private GameObject _currentItemStackGameObject;

        private UIItemSlotDrawer _currentDrawer;
        private Canvas _parentCanvas;
        private RectTransform _parentCanvasRT => _parentCanvas.transform as RectTransform;
        private RectTransform _rectTransform;
        private SlotContext _currentSlotContext;
        private bool _dragging = true;
        private bool _dragHandlersEnabled = true;

        public bool DraggingDisabled => !_dragging;
        public SlotContext CurrentSlotContext => _currentSlotContext;

        public event Action<UIItemSlotDrawer, UIItemSlotDragger> OnClick;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            _parentCanvas = GetComponentInParent<Canvas>();
            _currentDrawer = GetComponent<UIItemSlotDrawer>();
        }
        private void Update()
        {
            if (DraggingByClick && _draggingStartedByClickFromSlotDragger == this)
            {
                ContinueDrag(Mouse.current.position.ReadValue());
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
            if (DraggingByClick || _dragging == false) return;
            ProcessDrop(eventData.pointerDrag?.GetComponent<UIItemSlotDragger>());
        }
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke(_currentDrawer, this);

            if (_dragging == false) return;

            if (!DraggingByClick)
            {
                _dragHandlers.enabled = false;
                _draggingStartedByClickFromSlotDragger = this;
                StartDrag(eventData.position);
            }
            else
            {
                ProcessDrop(_draggingStartedByClickFromSlotDragger);
                _draggingStartedByClickFromSlotDragger.FinishDrag();
                _draggingStartedByClickFromSlotDragger = null;
                _dragHandlers.enabled = _dragHandlersEnabled;
            }
        }
        #endregion

        #region Drag'n Drop
        /// <summary>Начать перетаскивание (создаёт статический визуал и скрывает оригинал).</summary>
        public void StartDrag(Vector3 position)
        {
            DestroyDraggingStack();
            ToggleCurrentStackDrawer(false);

            
            DraggingStack = _currentDrawer.Stack.Clone();
            ItemInfo draggingInfo = _currentDrawer.ItemDatabase.Get(DraggingStack.Item.Id);

            _draggingStackGO = Instantiate(_draggingStackPrefab, _rectTransform.position, Quaternion.identity, _parentCanvas.transform);
            _draggingStackRT = _draggingStackGO.GetComponent<RectTransform>();
            _draggingStackDrawer = _draggingStackGO.GetComponent<UIImageWithLabel>();
            _draggingStackDrawer.SetUp(draggingInfo.Sprite, DraggingStack.Quantity.ToString());

            UpdateDraggedPosition(position);
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
            var fromContext = fromDragger.CurrentSlotContext;
            var toContext = this.CurrentSlotContext;

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
                fromContext.Inventory.MoveTo(toContext.Inventory, fromContext.SlotIndex, toContext.SlotIndex, DraggingStack.Quantity);
            }
        }
        #endregion

        #region Хелперы
        public void SetSlotContext(SlotContext context)
        {
            _currentSlotContext = context;
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

        private void UpdateDraggedPosition(Vector3 position)
        {
            if (_draggingStackRT == null) return;

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
            if (_currentItemStackGameObject != null)
                _currentItemStackGameObject.SetActive(active);
        }

        private void DestroyDraggingStack()
        {
            if (_draggingStackGO != null)
            {
                Destroy(_draggingStackGO);
                _draggingStackRT = null;
                _draggingStackGO = null;
                _draggingStackDrawer = null;
            }
        }
        #endregion
    }
}