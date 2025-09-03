using World.Items;

namespace World.Crafting
{
    // Система крафта предметов
    // Она позволяет работать не только с верстаком, но и с печками, наковальнями и другими рабочими блоками.
    public class CraftingTable : CraftSystem
    {
        public CraftingTable(ItemDatabase itemDatabase)
        {
            _itemDatabase = itemDatabase;
        }
    }
}