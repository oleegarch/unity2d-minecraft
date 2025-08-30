using System;
using UnityEngine;
using UnityEngine.UI;
using World.Items;

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
        [NonSerialized] public ItemDatabase ItemDatabase;

        public void SetUp(Inventory inventory, int slotIndex, ItemDatabase itemDatabase)
        {
            Inventory = inventory;
            SlotIndex = slotIndex;
            Stack = inventory.GetSlot(slotIndex);
            ItemDatabase = itemDatabase;
            Refresh();
        }

        public void Refresh(ItemStack stack)
        {
            if (!Stack.Equals(stack))
            {
                Stack = stack;
                Refresh();
            }
        }
        public void Refresh()
        {
            _stackDrawer.SetUp(Stack, ItemDatabase);
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