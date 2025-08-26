using System;
using UnityEngine;

namespace World.Inventories
{
    public class UIPlayerHotbarDrawer : MonoBehaviour, IDisposable
    {
        [SerializeField] private GameObject _uiItemSlotPrefab;

        private PlayerInventory _inventory;
        private UIItemSlotDrawer[] _uiItemSlots;

        public void SetUp(PlayerInventory inventory)
        {
            UnsubscribeFromInventoryEvents();
            Clear();

            _inventory = inventory;
            _uiItemSlots = new UIItemSlotDrawer[PlayerInventory.HotbarSize];

            SubscribeToInventoryEvents();

            RectTransform rt = transform as RectTransform;
            Vector2 size = rt.sizeDelta;
            size.x = PlayerInventory.HotbarSize * 32;
            rt.sizeDelta = size;

            for (int i = 0; i < PlayerInventory.HotbarSize; i++)
            {
                GameObject uiItemSlotGO = Instantiate(_uiItemSlotPrefab, transform);
                UIItemSlotDrawer uiItemSlot = uiItemSlotGO.GetComponent<UIItemSlotDrawer>();

                UIItemSlotDirection direction;
                if (i == 0)
                    direction = UIItemSlotDirection.Left;
                else if (i == PlayerInventory.HotbarSize - 1)
                    direction = UIItemSlotDirection.Right;
                else
                    direction = UIItemSlotDirection.Center;

                int hotbarIndex = inventory.HotbarIndexToSlot(i);
                uiItemSlot.SetUp(direction, inventory.GetSlot(hotbarIndex));
                _uiItemSlots[i] = uiItemSlot;
            }
        }

        // <summary> Обновляет все слоты HotBar'а. Сейчас не используется. </summary>
        public void Refresh()
        {
            for (int i = 0; i < _uiItemSlots.Length; i++)
            {
                int hotbarIndex = _inventory.HotbarIndexToSlot(i);
                _uiItemSlots[i].Refresh(_inventory.GetSlot(hotbarIndex));
            }
        }
        public void Refresh(int slotIndex, ItemStack newStack)
        {
            int hotbarIndex = _inventory.HotbarIndexToSlot(slotIndex);
            _uiItemSlots[hotbarIndex].Refresh(_inventory.GetSlot(hotbarIndex));
        }

        public int ChangeActiveHotbar(int newIndex)
        {
            if (_uiItemSlots.Length <= newIndex)
                newIndex = 0;
            else if (0 > newIndex)
                newIndex = _uiItemSlots.Length - 1;

            for (int i = 0; i < _uiItemSlots.Length; i++)
                _uiItemSlots[i].SetActive(newIndex == i);

            return newIndex;
        }

        public void Clear()
        {
            if (_uiItemSlots != null)
            {
                for (int i = 0; i < _uiItemSlots.Length; i++)
                    _uiItemSlots[i].Dispose();

                _uiItemSlots = null;
            }

            _inventory = null;
        }

        public void SubscribeToInventoryEvents()
        {
            if (_inventory == null) return;
            _inventory.Events.SlotChanged += Refresh;
        }
        public void UnsubscribeFromInventoryEvents()
        {
            if (_inventory == null) return;
            _inventory.Events.SlotChanged -= Refresh;
        }

        public void Dispose()
        {
            UnsubscribeFromInventoryEvents();
            Clear();
        }
    }
}