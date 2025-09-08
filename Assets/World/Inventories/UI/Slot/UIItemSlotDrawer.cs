using System;
using UIGlobal;
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
        [SerializeField] private UIHoverColorTransition _colorTransition;

        public UIImageWithLabel StackDrawer => _stackDrawer;
        public bool HasContent => _stackDrawer.GetCurrentImageSprite() != null && !string.IsNullOrEmpty(_stackDrawer.GetCurrentLabelText());

        public void SetUpStack(ItemDatabase itemDatabase, ItemStack stack)
        {
            if (stack.IsEmpty)
            {
                DisableStack();
                return;
            }

            ItemInfo itemInfo = itemDatabase.Get(stack.Item.Id);
            _stackDrawer.SetUp(itemInfo.Sprite, stack.Quantity.ToString());
        }
        public void SetUpStack(Sprite sprite, string quantityString)
        {
            _stackDrawer.SetUp(sprite, quantityString);
        }
        public void DisableStack()
        {
            _stackDrawer.SetUp(null, null);
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
            _colorTransition.enabled = false;
        }
        public void ClearColor()
        {
            _uiSlotImage.color = Color.white;
            _colorTransition.enabled = true;
        }

        public void Dispose()
        {
            Destroy(gameObject);
        }
    }
}