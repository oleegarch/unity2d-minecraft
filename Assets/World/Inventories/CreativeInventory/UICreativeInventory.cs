using System.Collections.Generic;
using TMPro;
using UnityEngine;
using World.Items;

namespace World.Inventories
{
    public class UICreativeInventory : MonoBehaviour
    {
        [SerializeField] private GameObject _categoryPrefab;
        [SerializeField] private Transform _categoriesBlocksParent;
        [SerializeField] private Transform _categoriesItemsParent;
        [SerializeField] private TextMeshProUGUI _title;

        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Transform _slotsParent;

        private ItemDatabase _itemDatabase;
        private ItemCategoryDatabase _itemCategoryDatabase;

        private List<GameObject> _instantiatedSlots = new();
        private List<GameObject> _instantiatedCategories = new();
        private ItemCategory _activeCategory;

        public void SetUp(ItemDatabase itemDatabase, ItemCategoryDatabase itemCategoryDatabase)
        {
            _itemDatabase = itemDatabase;
            _itemCategoryDatabase = itemCategoryDatabase;
            _activeCategory = itemCategoryDatabase.categories[0].Category;

            RecreateCategoriesButtons();
            SetActiveCategory();
        }

        public void SetActiveCategory(ItemCategory newCategory)
        {
            _activeCategory = newCategory;
            SetActiveCategory();
        }
        public void SetActiveCategory()
        {
            _title.SetText(_itemCategoryDatabase.Get(_activeCategory).Title);

            foreach (var instantiatedCategoryGO in _instantiatedCategories)
            {
                var instantiatedCategory = instantiatedCategoryGO.GetComponent<UICreativeInventoryCategory>();
                instantiatedCategory.SetActive(_activeCategory);
            }

            RecreateCategorySlots(_activeCategory);
        }
        public void RecreateCategoriesButtons()
        {
            DestroyCategoriesButtons();

            foreach (ItemCategoryInfo categoryInfo in _itemCategoryDatabase.categories)
            {
                var go = Instantiate(_categoryPrefab, categoryInfo.IsCategoryForBlocks ? _categoriesBlocksParent : _categoriesItemsParent);
                _instantiatedCategories.Add(go);

                var inventoryCategory = go.GetComponent<UICreativeInventoryCategory>();
                inventoryCategory.SetUp(categoryInfo);
                inventoryCategory.OnActiveSetAttempt += SetActiveCategory;
            }
        }
        private void RecreateCategorySlots(ItemCategory category)
        {
            DestroyCategorySlots();

            foreach (ItemInfo info in _itemDatabase.items)
            {
                if (info.Category == category)
                {
                    GameObject go = Instantiate(_slotPrefab, _slotsParent);

                    // Визуализируем слот
                    var drawer = go.GetComponent<UIItemSlotDrawer>();
                    var stack = new ItemStack(info, info.MaxStack);
                    drawer.SetUp(stack, _itemDatabase);

                    // Подписываемся на событие перетаскивания с креативного инвентаря
                    var dragger = go.GetComponent<UIItemSlotDragger>();
                    dragger.SetSlotContext(new SlotContext(stack));

                    _instantiatedSlots.Add(go);
                }
            }
        }

        public void DestroyCategorySlots()
        {
            foreach (var go in _instantiatedSlots)
                Destroy(go);
            _instantiatedSlots.Clear();
        }
        public void DestroyCategoriesButtons()
        {
            foreach (var go in _instantiatedCategories)
            {
                var inventoryCategory = go.GetComponent<UICreativeInventoryCategory>();
                inventoryCategory.OnActiveSetAttempt -= SetActiveCategory;
                Destroy(go);
            }
            _instantiatedCategories.Clear();
        }
    }
}