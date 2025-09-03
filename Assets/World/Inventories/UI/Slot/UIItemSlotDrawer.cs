using System;
using UnityEngine;
using UnityEngine.UI;
using World.Items;
using World.UI;

namespace World.Inventories
{
    public class UIItemSlotDrawer : MonoBehaviour
    {
        [Header("UI references")]
        [SerializeField] private Sprite _uiSlotSprite;
        [SerializeField] private Sprite _uiSlotActiveSprite;
        [SerializeField] private Image _uiSlotImage;
        [SerializeField] private UIImageWithLabel _stackDrawer;

        [NonSerialized] public ItemStack Stack;
        [NonSerialized] public ItemDatabase ItemDatabase;
        [NonSerialized] public bool Active;

        public void SetUp(ItemStack stack, ItemDatabase itemDatabase)
        {
            ItemDatabase = itemDatabase;
            Refresh(stack);
        }
        public void DisableCount()
        {
            _stackDrawer.DisableLabel();
        }

        public void Refresh(ItemStack stack)
        {
            if (Stack == null || !Stack.Equals(stack))
            {
                Stack = stack;

                if (!Stack.IsEmpty)
                {
                    ItemInfo info = ItemDatabase.Get(Stack.Item.Id);
                    _stackDrawer.SetUp(info.Sprite, Stack.Quantity.ToString());
                }
                else
                {
                    _stackDrawer.SetUp(null, null);
                }

                SetActive(Active);
            }
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
            Destroy(gameObject);
        }
    }
}