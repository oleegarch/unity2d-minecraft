using System;
using System.Linq;

namespace World.Inventories
{
    /// <summary>
    /// Специализация для хотбара: наследуемся от UIInventoryDrawer,
    /// всегда слушаем события (hotbar динамичен), добавляем активный слот и корректные направления краёв.
    /// </summary>
    public class UIPlayerHotbarDrawer : UIInventorySlotsDrawer
    {
        private PlayerInventory _playerInventory;
        private int _activeHotbarIndex = 0; // индекс внутри хотбара (0..HotbarSize-1)

        // Переопределяем SetUp для более удобного API
        public void SetUp(PlayerInventory inventory)
        {
            _playerInventory = inventory ?? throw new ArgumentNullException(nameof(inventory));

            // собираем абсолютные индексы хотбара
            var indices = Enumerable.Range(inventory.HotbarStart, PlayerInventory.HotbarSize);
            // alwaysUpdate = true — хотбар должен обновляться постоянно
            base.SetUp(inventory, indices, alwaysUpdate: true);
            // выведем активный слот по-умолчанию
            ChangeActiveHotbar(0);
        }

        /// <summary>
        /// Переключить активный слот (uiIndex внутри хотбара).
        /// Возвращает скорректированный индекс.
        /// </summary>
        public int ChangeActiveHotbar(int newIndex)
        {
            if (_uiItemSlots == null) return 0;

            if (newIndex >= _uiItemSlots.Length) newIndex = 0;
            if (newIndex < 0) newIndex = _uiItemSlots.Length - 1;

            for (int i = 0; i < _uiItemSlots.Length; i++)
                _uiItemSlots[i].SetActive(i == newIndex);

            _activeHotbarIndex = newIndex;
            return _activeHotbarIndex;
        }

        // доп. helper, если событие даёт глобальный индекс — можно конвертировать в хотбарный индекс
        private bool TryInventoryIndexToHotbarIndex(int inventoryIndex, out int hotbarIndex)
        {
            hotbarIndex = inventoryIndex - _playerInventory.HotbarStart;
            return hotbarIndex >= 0 && hotbarIndex < PlayerInventory.HotbarSize;
        }

        // Если нужно — можем переопределить RefreshSlotByInventoryIndex для оптимизированной логики
        protected override void RefreshSlotByInventoryIndex(int inventoryIndex, ItemStack newStack)
        {
            if (TryInventoryIndexToHotbarIndex(inventoryIndex, out int hotbarIndex))
                _uiItemSlots[hotbarIndex].Refresh(_playerInventory.GetSlot(inventoryIndex));
        }
    }
}