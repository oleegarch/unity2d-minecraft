using UnityEngine;
using World.Items;

namespace World.Inventories
{
    public class UICreativeInventory : MonoBehaviour, IUIInventoryAccessor
    {
        [SerializeField] private UIItemCategoriesDrawer _categoriesDrawer;
        [SerializeField] private UIItemCategorSlotsDrawer _categorySlotsDrawer;

        private void OnEnable()
        {
            _categorySlotsDrawer.OnSlotCreated += OnSlotCreated;
        }
        private void OnDisable()
        {
            _categorySlotsDrawer.OnSlotCreated -= OnSlotCreated;
        }

        public void SetUp(ItemDatabase itemDatabase, ItemCategoryDatabase itemCategoryDatabase)
        {
            _categorySlotsDrawer.SetUp(itemDatabase);
            _categoriesDrawer.SetUp(itemCategoryDatabase);
        }
        
        public void OnSlotCreated(UIItemSlotDrawer drawer, UIItemSlotDragger dragger)
        {
            drawer.DisableCount();
            dragger.SetSlotContext(new SlotContext(drawer.Stack, SlotType.Creative));
            dragger.DisableDragHandlers();
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