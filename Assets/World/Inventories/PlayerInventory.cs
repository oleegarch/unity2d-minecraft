using System.Collections.Generic;
using System.Linq;

namespace World.Inventories
{
    public sealed class PlayerInventory : Inventory
    {
        public const int HotbarSize = 10;
        public const int MainSize = 30;
        public int HotbarStart => 0;
        public int MainStart => HotbarStart + HotbarSize;

        public PlayerInventory() : base(HotbarSize + MainSize) { }

        public int HotbarIndexToSlot(int hotbarIndex) => HotbarStart + hotbarIndex;

        public IEnumerable<ItemStack> HotbarSlots() => Enumerable.Range(HotbarStart, HotbarSize).Select(i => GetSlot(i));
        public IEnumerable<ItemStack> MainSlots() => Enumerable.Range(MainStart, MainSize).Select(i => GetSlot(i));
    }
}