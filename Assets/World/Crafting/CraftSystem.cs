using System;
using System.Linq;
using System.Collections.Generic;
using World.Items;
using World.Inventories;

namespace World.Crafting
{
    // Система крафта предметов
    // Она позволяет работать не только с верстаком, но и с печками, наковальнями и другими рабочими блоками.
    public abstract class CraftSystem
    {
        protected ItemDatabase _itemDatabase;
        protected InventoryType _inventoryType;
        public InventoryType InventoryType => _inventoryType;

        // Абстрактный метод для крафта конкретного предмета по определённому варианту крафта
        public abstract bool Craft(Inventory inventory, Inventory returnInventory, ItemInfo item, byte variantId);

        #region Забрать ингредиенты
        // Забрать все ингредиенты для крафта с инвентаря по конкретному варианту крафта предварительно проверив их наличие
        public bool TakeIngredients(Inventory inventory, CraftVariant variant)
        {
            if (!CheckAvailabilityVariant(inventory, variant)) return false;

            foreach (CraftIngredient ingredient in variant.Ingredients)
            {
                if (!TakeIngredient(inventory, ingredient))
                    return false;
            }

            return true;
        }
        // Забрать ингредиент для крафта с инвентаря
        public bool TakeIngredient(Inventory inventory, CraftIngredient ingredient)
        {
            switch (ingredient.Type)
            {
                case CraftIngredientType.ExactlyItem:
                    {
                        ushort itemId = _itemDatabase.GetId(ingredient.ItemName);
                        return inventory.Take(itemId, ingredient.Quantity);
                    }
                case CraftIngredientType.TypeItem:
                    {
                        List<ItemInfo> sameType = GetItemsWithSameCraftingType(ingredient.ItemType);
                        foreach (ItemInfo item in sameType)
                        {
                            if (inventory.Take(item.Id, ingredient.Quantity))
                                return true;
                        }
                        return false;
                    }
                default:
                    {
                        throw new NotImplementedException($"The TakeGradient method is not implemented for CraftIngredientType=={ingredient.Type.ToString()}");
                    }
            }
        }
        #endregion

        #region Проверка наличия
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
                switch (ingredient.Type)
                {
                    case CraftIngredientType.ExactlyItem:
                        {
                            if (!inventory.Has(_itemDatabase.GetId(ingredient.ItemName), ingredient.Quantity))
                                return false;
                            continue;
                        }
                    case CraftIngredientType.TypeItem:
                        {
                            List<ItemInfo> sameType = GetItemsWithSameCraftingType(ingredient.ItemType);
                            
                            bool has = false;
                            foreach (ItemInfo item in sameType)
                            {
                                if (inventory.Has(item.Id, ingredient.Quantity))
                                {
                                    has = true;
                                    break;
                                }
                            }

                            if (has) continue;

                            return false;
                        }
                    default:
                        {
                            throw new NotImplementedException($"The CheckAvailabilityVariant method is not implemented for CraftIngredientType=={ingredient.Type.ToString()}");
                        }
                }
            }

            return true;
        }
        #endregion

        #region Хелперы
        // Вернуть предметы с таким же типом крафта
        public List<ItemInfo> GetItemsWithSameCraftingType(string craftingType)
        {
            return _itemDatabase.items.Where(item => item.CraftVariants.ItemType == craftingType).ToList();
        }
        #endregion
    }
}