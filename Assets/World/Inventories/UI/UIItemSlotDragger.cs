using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Inventories
{
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

        public ItemStack StaticStack => _staticStackDrawer.Stack;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
            _parentCanvas = GetComponentInParent<Canvas>();
            _currentDrawer = GetComponent<UIItemSlotDrawer>();
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
            GameObject fromGO = eventData.pointerDrag;

            var fromDragger = fromGO?.GetComponent<UIItemSlotDragger>();
            var fromDrawer = fromGO?.GetComponent<UIItemSlotDrawer>();
            var toDragger = this;
            var toDrawer = _currentDrawer;
            
            if (fromGO == null || fromDragger == null || fromDrawer == this) return;

            fromDrawer.Inventory.MoveTo(toDrawer.Inventory, fromDrawer.SlotIndex, toDrawer.SlotIndex, fromDragger.StaticStack.Quantity);
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