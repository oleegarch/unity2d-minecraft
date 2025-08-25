using UnityEngine;
using UnityEngine.UI;

namespace World.Inventories
{
    public enum UIItemSlotDirection
    {
        Left,
        Center,
        Right
    }
    public class UIItemSlotDrawer : MonoBehaviour
    {
        [SerializeField] private Sprite _leftItemSlot;
        [SerializeField] private Sprite _centerItemSlot;
        [SerializeField] private Sprite _rightItemSlot;
        [SerializeField] private Image _uiItemSlotImage;
        [SerializeField] private Image _uiItemImage;

        public void SetUp(UIItemSlotDirection direction, ItemStack stack)
        {
            switch (direction)
            {
                case UIItemSlotDirection.Left:
                {
                    _uiItemSlotImage.sprite = _leftItemSlot;
                    break;
                }
                case UIItemSlotDirection.Center:
                {
                    _uiItemSlotImage.sprite = _centerItemSlot;
                    break;
                }
                case UIItemSlotDirection.Right:
                {
                    _uiItemSlotImage.sprite = _rightItemSlot;
                    break;
                }
            }
            if (stack.Item != null)
            {
                _uiItemImage.sprite = stack.Item.Sprite;
                _uiItemImage.enabled = true;
            }
            else
            {
                _uiItemImage.enabled = false;
            }
        }
    }
}