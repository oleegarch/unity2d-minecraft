using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace World.Inventories
{
    public enum SlotType
    {
        Creative,
        Default
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
        public SlotContext(ItemStack stack)
        {
            ItemStack = stack;
            SlotType = SlotType.Creative;
        }
    }

    public class UIItemSlotDragger : MonoBehaviour, IPointerClickHandler, IDropHandler
    {
        private static UIItemSlotDragger _draggingStartedByClickFromSlotDragger;
        private static GameObject _draggingStackGO;
        private static RectTransform _draggingStackRT;
        private static UIStackSlotDrawer _draggingStackDrawer;
        public static bool DraggingByClick => _draggingStartedByClickFromSlotDragger != null;
        public static ItemStack DraggingStack => _draggingStackDrawer?.Stack;

        [SerializeField] private UIItemSlotDragHandlers _dragHandlers;
        [SerializeField] private GameObject _draggingStackPrefab;
        [SerializeField] private GameObject _currentStackGO;

        private UIItemSlotDrawer _currentDrawer;
        private Canvas _parentCanvas;
        private RectTransform _parentCanvasRT => _parentCanvas.transform as RectTransform;
        private RectTransform _rectTransform;
        private SlotContext _currentSlotContext;
        private bool _dragHandlersEnabled = true;

        public SlotContext CurrentSlotContext => _currentSlotContext;

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
            if (DraggingByClick) return;
            ProcessDrop(eventData.pointerDrag?.GetComponent<UIItemSlotDragger>());
        }
        public void OnPointerClick(PointerEventData eventData)
        {
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

            _draggingStackGO = Instantiate(_draggingStackPrefab, _rectTransform.position, Quaternion.identity, _parentCanvas.transform);
            _draggingStackRT = _draggingStackGO.GetComponent<RectTransform>();
            _draggingStackDrawer = _draggingStackGO.GetComponent<UIStackSlotDrawer>();
            _draggingStackDrawer.SetUp(_currentDrawer.Stack, _currentDrawer.ItemDatabase);

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

            if (fromContext == null || toContext == null) return;

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
            if (_currentStackGO != null)
                _currentStackGO.SetActive(active);
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