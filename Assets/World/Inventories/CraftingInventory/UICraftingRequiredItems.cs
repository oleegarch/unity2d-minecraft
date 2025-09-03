using System.Collections.Generic;
using UnityEngine;
using World.UI;
using World.Crafting;
using World.Items;
using System.Linq;

namespace World.Inventories
{
    public class UICraftingRequiredItems : MonoBehaviour
    {
        [SerializeField] private Transform _requiredItemsParent;
        [SerializeField] private GameObject _requiredItemPrefab;

        private List<UIImageWithLabel> _instantiatedItems = new();

        public void SetUp(ItemInfo item, ItemDatabase itemDatabase, CraftSystem system)
        {
            DestroyInstantiatedItems();

            foreach (CraftVariant variant in item.CraftVariants.GetVariants(system.InventoryType))
            {
                foreach (CraftIngredient ingredient in variant.Ingredients)
                {
                    GameObject requiredGO = Instantiate(_requiredItemPrefab, _requiredItemsParent);
                    UIImageWithLabel required = requiredGO.GetComponent<UIImageWithLabel>();
                    _instantiatedItems.Add(required);
        
                    switch (ingredient.Type)
                    {
                        case CraftIngredientType.ExactlyItem:
                            {
                                ItemInfo info = itemDatabase.GetByName(ingredient.ItemName);
                                required.SetUp(info.Sprite, info.Title, ingredient.Quantity.ToString());
                                _instantiatedItems.Add(required);
                                break;
                            }
                        case CraftIngredientType.TypeItem:
                            {
                                List<ItemInfo> infos = system.GetItemsWithSameCraftingType(ingredient.ItemType);
                                required.SetUp(null, null, ingredient.Quantity.ToString());
                                required.StartChangingAnimation(
                                    infos.Select(info => info.Sprite).ToList(),
                                    infos.Select(info => info.Title).ToList()
                                );
                                break;
                            }
                    }
                }
            }
        }

        public void DestroyInstantiatedItems()
        {
            foreach (UIImageWithLabel item in _instantiatedItems)
                Destroy(item.gameObject);

            _instantiatedItems.Clear();
        }
    }
}