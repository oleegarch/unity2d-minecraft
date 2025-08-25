using UnityEngine;

namespace World.Inventories
{
    public class UIPlayerHotbarDrawer : MonoBehaviour
    {
        [SerializeField] private GameObject _uiItemSlotPrefab;

        private UIItemSlotDrawer[] _uiItemSlots;

        public void SetUp(PlayerInventory inventory)
        {
            _uiItemSlots = new UIItemSlotDrawer[PlayerInventory.HotbarSize];

            RectTransform rt = transform as RectTransform;
            Vector2 size = rt.sizeDelta;
            size.x = PlayerInventory.HotbarSize * 32;
            rt.sizeDelta = size;

            for (int i = 0; i < PlayerInventory.HotbarSize; i++)
            {
                GameObject uiItemSlotGO = Instantiate(_uiItemSlotPrefab, transform);
                UIItemSlotDrawer uIItemSlot = uiItemSlotGO.GetComponent<UIItemSlotDrawer>();

                UIItemSlotDirection direction;
                if (i == 0)
                    direction = UIItemSlotDirection.Left;
                else if (i == PlayerInventory.HotbarSize - 1)
                    direction = UIItemSlotDirection.Right;
                else
                    direction = UIItemSlotDirection.Center;

                int hotbarIndex = inventory.HotbarIndexToSlot(i);
                uIItemSlot.SetUp(direction, inventory.GetSlot(hotbarIndex));
                _uiItemSlots[i] = uIItemSlot;
            }
        }
    }
}