using System.Collections.Generic;
using World.Items;
using World.Inventories;

namespace World.Crafting
{
    // Система крафта предметов
    // Она позволяет работать не только с верстаком, но и с печками, наковальнями и другими рабочими блоками.
    public class CraftSystem
    {
        private ItemDatabase _itemDatabase;

        public CraftSystem(ItemDatabase itemDatabase)
        {
            _itemDatabase = itemDatabase;
        }

        // Отфильтровать варианты крафтов и вернуть те которые можно скрафтить
        public List<CraftVariant> SelectAvailabilityVariants(Inventory inventory, CraftVariants variants)
        {
            var availabilityVariants = new List<CraftVariant>();

            foreach (var variant in variants)
            {
                if (!CheckAvailabilityVariant(inventory, variant)) continue;
                availabilityVariants.Add(variant);
            }

            return availabilityVariants;
        }

        // Проверяет можно ли скрафтить предмет по определённому варианту крафта
        public bool CheckAvailabilityVariant(Inventory inventory, CraftVariant variant)
        {
            foreach (var ingredient in variant.Ingredients)
            {
                if (!inventory.Has(_itemDatabase.GetId(ingredient.ItemName), ingredient.Quantity))
                    return false;
            }

            return true;
        }
    }
}