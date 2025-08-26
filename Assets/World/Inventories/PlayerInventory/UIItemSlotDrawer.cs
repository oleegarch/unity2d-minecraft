using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace World.Inventories
{
    [Flags]
    public enum UIItemSlotDirection
    {
        None = 0,

        // позиции (низкие биты)
        Left = 1 << 0,
        XCenter = 1 << 1,
        Right = 1 << 2,
        Top = 1 << 3,
        YCenter = 1 << 4,
        Bottom = 1 << 5,

        // типы (выделим старшие биты, чтобы не пересекались с позициями)
        OneRow = 1 << 8,
        MultiRow = 1 << 9,
    }

    public static class UIItemSlotDirectionExtensions
    {
        public static bool IsOneRow(this UIItemSlotDirection d) => (d & UIItemSlotDirection.OneRow) != 0;
        public static bool IsMultiRow(this UIItemSlotDirection d) => (d & UIItemSlotDirection.MultiRow) != 0;

        public static bool HasPosition(this UIItemSlotDirection d, UIItemSlotDirection pos) => (d & pos) != 0;
    }

    [Serializable]
    public class SlotSpriteEntry
    {
        [Tooltip("Тип: OneRow или MultiRow. Выбирайте только типовые флаги (можно несколько, но обычно один).")]
        public UIItemSlotDirection Type = UIItemSlotDirection.None;

        [Tooltip("Позиция: Left / Center / Right / Top / Bottom. Обычно указывайте один флаг.")]
        public UIItemSlotDirection Position = UIItemSlotDirection.None;

        [Tooltip("Спрайт для неактивного состояния")]
        public Sprite Normal;

        [Tooltip("Спрайт для активного состояния (если не указан — берётся Normal)")]
        public Sprite Active;
    }

    public class UIItemSlotDrawer : MonoBehaviour
    {
        [Header("Sprite entries — заполнить в инспекторе")]
        [SerializeField] private List<SlotSpriteEntry> _spriteEntries = new();

        [Header("Fallback sprites (если не найдена запись)")]
        [SerializeField] private Sprite _defaultNormal;
        [SerializeField] private Sprite _defaultActive;

        [Header("UI references")]
        [SerializeField] private Image _uiItemSlotImage;
        [SerializeField] private Image _uiItemImage;
        [SerializeField] private TextMeshProUGUI _uiTextCount;

        private UIItemSlotDirection _currentDirection = UIItemSlotDirection.None;
        private ItemStack _currentStack;
        private bool _currentActive;

        public void SetUp(UIItemSlotDirection direction, ItemStack stack)
        {
            _currentDirection = direction;
            _currentStack = stack;

            SetActive(_currentActive); // установит спрайт рамки

            if (stack?.Item?.Sprite != null)
            {
                _uiItemImage.sprite = stack.Item.Sprite;
                _uiItemImage.enabled = true;
            }
            else
            {
                _uiItemImage.enabled = false;
            }

            if (stack?.Count > 0)
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
            var sprite = GetSpriteFor(_currentDirection, isActive);
            if (_uiItemSlotImage != null)
                _uiItemSlotImage.sprite = sprite;
        }

        public void Dispose()
        {
            _currentStack = null;
            _currentActive = false;
            Destroy(gameObject);
        }

        // Берем подходящий элемент в _spriteEntries
        private Sprite GetSpriteFor(UIItemSlotDirection direction, bool active)
        {
            // Если список пуст — вернуть дефолты
            if (_spriteEntries == null || _spriteEntries.Count == 0)
                return active ? (_defaultActive ?? _defaultNormal) : _defaultNormal;

            // Маска позиционных битов (защита от случайного указания Type в поле Position)
            const UIItemSlotDirection positionMask =
                UIItemSlotDirection.Left
                | UIItemSlotDirection.XCenter
                | UIItemSlotDirection.Right
                | UIItemSlotDirection.Top
                | UIItemSlotDirection.YCenter
                | UIItemSlotDirection.Bottom;

            foreach (var e in _spriteEntries)
            {
                // Type совпадает, если запись не указывает Type, либо все указанные Type-биты содержатся в direction
                bool typeMatches = (e.Type == UIItemSlotDirection.None) || ((direction & e.Type) == e.Type);

                if (!typeMatches) continue;

                // Position совпадает, если запись не указывает Position, либо все позиционные биты записи содержатся в direction
                var entryPos = e.Position & positionMask;
                bool posMatches = (entryPos == UIItemSlotDirection.None) || ((direction & entryPos) == entryPos);

                if (!posMatches) continue;

                // Первый подходящий — возвращаем сразу (с учётом fallback active->normal и глобальных дефолтов)
                if (active)
                    return e.Active ?? e.Normal ?? (_defaultActive ?? _defaultNormal);
                else
                    return e.Normal ?? (_defaultActive ?? _defaultNormal) ?? _defaultNormal;
            }

            // Если вдруг ничего не нашлось — дефолты
            return active ? (_defaultActive ?? _defaultNormal) : _defaultNormal;
        }

    }
}