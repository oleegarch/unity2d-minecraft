using System;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public class UIItemSlotDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private GameObject _staticStackPrefab;
        [SerializeField] private GameObject _currentStackGO;

        private UIItemSlotDrawer _currentDrawer;
        private UIStackSlotDrawer _staticStackDrawer;
        private Canvas _parentCanvas;
        private RectTransform _parentCanvasRT => _parentCanvas.transform as RectTransform;
        private GameObject _staticStackGO;
        private RectTransform _staticStackRT;
        private RectTransform _rectTransform;
        private SlotContext _currentSlotContext;

        public ItemStack StaticStack => _staticStackDrawer.Stack;
        public SlotContext CurrentSlotContext => _currentSlotContext;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            _parentCanvas = GetComponentInParent<Canvas>();
            _currentDrawer = GetComponent<UIItemSlotDrawer>();
        }

        public void SetSlotContext(SlotContext context)
        {
            _currentSlotContext = context;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            DestroyStatic();
            ToggleCurrentStackDrawer(false);

            _staticStackGO = Instantiate(_staticStackPrefab, _rectTransform.position, Quaternion.identity, _parentCanvas.transform);
            _staticStackRT = _staticStackGO.GetComponent<RectTransform>();
            _staticStackDrawer = _staticStackGO.GetComponent<UIStackSlotDrawer>();
            _staticStackDrawer.SetUp(_currentDrawer.Stack, _currentDrawer.ItemDatabase);

            UpdateDraggedPosition(eventData);
        }
        public void OnDrag(PointerEventData eventData)
        {
            UpdateDraggedPosition(eventData);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            DestroyStatic();
            ToggleCurrentStackDrawer(true);
        }
        public void OnDrop(PointerEventData eventData)
        {
            var fromGO = eventData.pointerDrag;

            var fromDragger = fromGO?.GetComponent<UIItemSlotDragger>();
            var fromContext = fromDragger.CurrentSlotContext;
            var toDragger = this;
            var toContext = toDragger.CurrentSlotContext;

            if (fromGO == null || fromDragger == null || fromContext == null || toContext == null)
                throw new ArgumentNullException(nameof(fromGO));

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
                fromContext.Inventory.MoveTo(toContext.Inventory, fromContext.SlotIndex, toContext.SlotIndex, fromDragger.StaticStack.Quantity);
            }
        }

        private void UpdateDraggedPosition(PointerEventData eventData)
        {
            if (_staticStackRT == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvasRT,
                eventData.position,
                _parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : _parentCanvas.worldCamera,
                out var localPos))
            {
                _staticStackRT.localPosition = localPos;
            }
        }

        private void ToggleCurrentStackDrawer(bool active)
        {
            _currentStackGO.SetActive(active);
        }

        private void DestroyStatic()
        {
            if (_staticStackGO != null)
            {
                Destroy(_staticStackGO);
                _staticStackRT = null;
                _staticStackGO = null;
                _staticStackDrawer = null;
            }
        }
    }
}