using UnityEngine;
using World.Items;

namespace World.Inventories
{
    public class UICraftingInventory : MonoBehaviour, IUIInventoryAccessor
    {
        [SerializeField] private UIItemCategoriesDrawer _categoriesDrawer;
        [SerializeField] private UIItemCategorSlotsDrawer _categorySlotsDrawer;
        [SerializeField] private Transform _requiredItemsParent;
        [SerializeField] private GameObject _requiredItemPrefab;

        public void SetUp(ItemDatabase itemDatabase, ItemCategoryDatabase itemCategoryDatabase)
        {
            _categorySlotsDrawer.SetUp(itemDatabase);
            _categoriesDrawer.SetUp(itemCategoryDatabase);
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }
        public void Close()
        {
            gameObject.SetActive(false);
        }
        public bool Toggle()
        {
            bool newActive = !gameObject.activeSelf;
            gameObject.SetActive(newActive);
            return newActive;
        }

        public void Dispose()
        {
            _categoriesDrawer.Dispose();
            _categorySlotsDrawer.Dispose();
            Destroy(gameObject);
        }
    }
}