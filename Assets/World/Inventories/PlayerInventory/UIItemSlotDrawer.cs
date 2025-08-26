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
        [SerializeField] private Sprite _leftItemSlotActive;
        [SerializeField] private Sprite _centerItemSlotActive;
        [SerializeField] private Sprite _rightItemSlotActive;
        [SerializeField] private Image _uiItemSlotImage;
        [SerializeField] private Image _uiItemImage;
        [SerializeField] private TextMeshProUGUI _uiTextCount;

        private UIItemSlotDirection _currentDirection;
        private ItemStack _currentStack;
        private bool _currentActive;

        public void SetUp(UIItemSlotDirection direction, ItemStack stack)
        {
            _currentDirection = direction;
            _currentStack = stack;

            SetActive(_currentActive);

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
        public void SetActive(bool isActive)
        {
            _currentActive = isActive;

            switch (_currentDirection)
            {
                case UIItemSlotDirection.Left:
                    {
                        _uiItemSlotImage.sprite = isActive ? _leftItemSlotActive : _leftItemSlot;
                        break;
                    }
                case UIItemSlotDirection.Center:
                    {
                        _uiItemSlotImage.sprite = isActive ? _centerItemSlotActive : _centerItemSlot;
                        break;
                    }
                case UIItemSlotDirection.Right:
                    {
                        _uiItemSlotImage.sprite = isActive ? _rightItemSlotActive : _rightItemSlot;
                        break;
                    }
            }
        }

        public void Dispose()
        {
            _currentStack = null;
            _currentActive = false;
            Destroy(gameObject);
        }
    }
}