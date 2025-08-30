using System;
using UnityEngine;
using UnityEngine.UI;

namespace World.Inventories
{
    public class UIItemSlotDrawer : MonoBehaviour
    {
        [Header("UI references")]
        [SerializeField] private Sprite _uiSlotSprite;
        [SerializeField] private Sprite _uiSlotActiveSprite;
        [SerializeField] private Image _uiSlotImage;
        [SerializeField] private UIStackSlotDrawer _stackDrawer;

        [NonSerialized] public ItemStack Stack;
        [NonSerialized] public int SlotIndex;
        [NonSerialized] public bool Active;
        [NonSerialized] public Inventory Inventory;

        public void SetUp(Inventory inventory, int slotIndex)
        {
            Inventory = inventory;
            SlotIndex = slotIndex;
            Stack = inventory.GetSlot(slotIndex);
            Refresh();
        }

        public void Refresh(ItemStack stack)
        {
            if (Stack == null) throw new InvalidOperationException($"SetUp not called for Refresh");

            if (
                Stack.Item?.Sprite != stack.Item?.Sprite ||
                Stack.Count != stack.Count
            )
            {
                Stack = stack;
                Refresh();
            }
        }
        public void Refresh()
        {
            _stackDrawer.SetUp(Stack);
            SetActive(Active);
        }

        public void SetActive(bool isActive)
        {
            Active = isActive;

            _uiSlotImage.sprite = isActive ? _uiSlotActiveSprite : _uiSlotSprite;
        }

        public void Dispose()
        {
            Stack = null;
            Active = false;
            _stackDrawer.Dispose();
            Destroy(gameObject);
        }
    }
}