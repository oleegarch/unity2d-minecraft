using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using World.Items;

namespace World.Inventories
{
    public class UIItemCategoriesDrawer : MonoBehaviour
    {
        [SerializeField] private GameObject _categoryPrefab;
        [SerializeField] private Transform _categoriesBlocksParent;
        [SerializeField] private Transform _categoriesItemsParent;
        [SerializeField] private TextMeshProUGUI _title;

        private ItemCategory _activeCategory;
        private ItemCategoryDatabase _itemCategoryDatabase;
        private List<GameObject> _instantiatedCategories = new();

        public event Action<ItemCategory> OnCategoryChanged;
        
        private void OnDestroy()
        {
            DestroyCategoriesButtons();
        }

        public void SetUp(ItemCategoryDatabase itemCategoryDatabase)
        {
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
                var instantiatedCategory = instantiatedCategoryGO.GetComponent<UIItemCategory>();
                instantiatedCategory.SetActive(_activeCategory);
            }

            OnCategoryChanged?.Invoke(_activeCategory);
        }
        public void RecreateCategoriesButtons()
        {
            DestroyCategoriesButtons();

            foreach (ItemCategoryInfo categoryInfo in _itemCategoryDatabase.categories)
            {
                var go = Instantiate(_categoryPrefab, categoryInfo.IsCategoryForBlocks ? _categoriesBlocksParent : _categoriesItemsParent);
                _instantiatedCategories.Add(go);

                var inventoryCategory = go.GetComponent<UIItemCategory>();
                inventoryCategory.SetUp(categoryInfo);
                inventoryCategory.OnActiveSetAttempt += SetActiveCategory;
            }
        }
        public void DestroyCategoriesButtons()
        {
            foreach (var go in _instantiatedCategories)
            {
                var inventoryCategory = go.GetComponent<UIItemCategory>();
                inventoryCategory.OnActiveSetAttempt -= SetActiveCategory;
                Destroy(go);
            }
            _instantiatedCategories.Clear();
        }
        public void Dispose()
        {
            DestroyCategoriesButtons();
            Destroy(gameObject);
        }
    }
}