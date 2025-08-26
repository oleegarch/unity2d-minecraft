using System;
using System.Linq;

namespace World.Inventories
{
    /// <summary>
    /// Drawer для Main-slots игрока. Строится заранее, но не подписывается на события
    /// и не обновляется пока пользователь не откроет инвентарь (т.е. вызывает Open()).
    /// </summary>
    public class UIPlayerMainSlotsDrawer : UIInventorySlotsDrawer
    {
        private PlayerInventory _playerInventory;

        /// <summary>
        /// Инициализация: собирает абсолютные индексы main-слотов,
        /// не включает alwaysUpdate — обновления начнутся при Open().
        /// </summary>
        public void SetUp(PlayerInventory inventory)
        {
            _playerInventory = inventory ?? throw new ArgumentNullException(nameof(inventory));

            var indices = Enumerable.Range(inventory.MainStart, PlayerInventory.MainSize);
            // alwaysUpdate = false — обновляем только когда пользователь откроет инвентарь
            base.SetUp(inventory, indices, alwaysUpdate: false);
        }
    }
}