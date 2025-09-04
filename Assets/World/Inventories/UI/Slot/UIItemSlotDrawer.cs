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

        public UIImageWithLabel StackDrawer => _stackDrawer;

        public void SetUpStack(ItemDatabase itemDatabase, ItemStack stack)
        {
            if (!stack.IsEmpty)
            {
                var itemInfo = itemDatabase.Get(stack.Item.Id);
                _stackDrawer.SetUp(itemInfo.Sprite, stack.Quantity.ToString());
            }
            else
            {
                _stackDrawer.SetUp(null, null);
            }
        }
        public void SetUpStack(Sprite sprite, string quantityString)
        {
            _stackDrawer.SetUp(sprite, quantityString);
        }

        public void ToggleCountLabel(bool enabled)
        {
            _stackDrawer.ToggleLabel(enabled);
        }

        public void SetActive(bool isActive)
        {
            _uiSlotImage.sprite = isActive ? _uiSlotActiveSprite : _uiSlotSprite;
        }

        public void SetColor(Color color)
        {
            _uiSlotImage.color = color;
        }

        public void Dispose()
        {
            Destroy(gameObject);
        }
    }
}