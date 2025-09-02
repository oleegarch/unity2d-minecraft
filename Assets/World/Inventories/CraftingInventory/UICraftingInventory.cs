using UnityEngine;
using World.Items;

namespace World.Inventories
{
    public class UICraftingInventory : MonoBehaviour, IUIInventoryAccessor
    {
        [SerializeField] private Transform _slotsParent;
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Transform _categoriesParent;
        [SerializeField] private GameObject _categoryPrefab;
        [SerializeField] private Transform _requiredItemsParent;
        [SerializeField] private GameObject _requiredItemPrefab;

        public void SetUp(ItemDatabase itemDatabase, ItemCategoryDatabase itemCategoryDatabase)
        {

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
            Destroy(gameObject);
        }
    }
}