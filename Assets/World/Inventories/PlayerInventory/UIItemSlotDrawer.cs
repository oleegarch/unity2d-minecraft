using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private UIStackSlotDrawer _stackDrawer;

        [NonSerialized] public UIItemSlotDirection Direction = UIItemSlotDirection.None;
        [NonSerialized] public ItemStack Stack;
        [NonSerialized] public int SlotIndex;
        [NonSerialized] public bool Active;
        [NonSerialized] public Inventory Inventory;

        public void SetUp(UIItemSlotDirection direction, Inventory inventory, int slotIndex)
        {
            Direction = direction;
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
            _uiItemSlotImage.sprite = GetSpriteFor(Direction, isActive);
        }

        public void Dispose()
        {
            Stack = null;
            Active = false;
            _stackDrawer.Dispose();
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