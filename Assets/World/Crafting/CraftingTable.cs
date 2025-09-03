using World.Inventories;
using World.Items;

namespace World.Crafting
{
    // Система крафта предметов
    // Она позволяет работать не только с верстаком, но и с печками, наковальнями и другими рабочими блоками.
    public class CraftingTable : CraftSystem
    {
        public CraftingTable(ItemDatabase itemDatabase, InventoryType inventoryType)
        {
            _itemDatabase = itemDatabase;
            _inventoryType = inventoryType;
        }

        public override bool Craft(Inventory inventory, Inventory returnInventory, ItemInfo item, byte variantId)
        {
            CraftVariant variant = item.CraftVariants.GetVariantById(variantId);

            if (returnInventory.HasEmptySlot && TakeIngredients(inventory, variant))
            {
                ItemStack stack = new ItemStack(item, variant.ReturnCount);
                return returnInventory.TryAdd(stack);
            }

            return false;
        }
    }
}