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

        private PlayerInventory _playerInventory;

        private void OnEnable()
        {
            _categorySlotsDrawer.OnSlotCreated += OnSlotCreated;
            _categorySlotsDrawer.OnSlotDestroy += OnSlotDestroy;
        }
        private void OnDisable()
        {
            _categorySlotsDrawer.OnSlotCreated -= OnSlotCreated;
            _categorySlotsDrawer.OnSlotDestroy -= OnSlotDestroy;
        }

        public void SetUp(PlayerInventory inventory, ItemDatabase itemDatabase, ItemCategoryDatabase itemCategoryDatabase)
        {
            _playerInventory = inventory;
            _categorySlotsDrawer.SetUp(itemDatabase);
            _categoriesDrawer.SetUp(itemCategoryDatabase);
        }

        public void OnSlotCreated(UIItemSlotDrawer drawer, UIItemSlotDragger dragger)
        {
            drawer.DisableCount();
            dragger.SetSlotContext(new SlotContext(drawer.Stack, SlotType.Preview));
            dragger.DisableDragging();
            dragger.OnClick += OnSlotClicked;
        }
        public void OnSlotDestroy(UIItemSlotDrawer drawer, UIItemSlotDragger dragger)
        {
            dragger.OnClick -= OnSlotClicked;
        }
        public void OnSlotClicked(UIItemSlotDrawer drawer, UIItemSlotDragger dragger)
        {
            Debug.Log($"Clicked to slot {drawer.Stack}");
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
            _categorySlotsDrawer.Dispose();
            _categoriesDrawer.Dispose();
            Destroy(gameObject);
        }
    }
}