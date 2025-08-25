using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

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
        [SerializeField] private TextMeshProUGUI _uiTextCount;

        private UIItemSlotDirection _currentDirection;
        private ItemStack _currentStack;

        public void SetUp(UIItemSlotDirection direction, ItemStack stack)
        {
            _currentDirection = direction;
            _currentStack = stack;

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

            if (stack.Count > 0)
            {
                _uiTextCount.SetText(stack.Count.ToString());
                _uiTextCount.enabled = true;
            }
            else
            {
                _uiTextCount.enabled = false;
            }
        }

        public void Refresh(ItemStack stack)
        {
            if (_currentStack == null) throw new InvalidOperationException($"SetUp not called for Refresh");
            
            if (
                _currentStack.Item?.Sprite != stack.Item?.Sprite ||
                _currentStack.Count != stack.Count
            )
            {
                SetUp(_currentDirection, stack);
            }
        }

        public void Dispose()
        {
            _currentStack = null;
            Destroy(gameObject);
        }
    }
}