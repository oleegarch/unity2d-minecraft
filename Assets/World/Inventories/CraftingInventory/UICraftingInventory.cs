using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using World.UI;
using World.Crafting;
using World.Items;

namespace World.Inventories
{
    public class UICraftingInventory : MonoBehaviour, IUIInventoryAccessor
    {
        [SerializeField] private UIItemCategoriesDrawer _categoriesDrawer;
        [SerializeField] private UIItemCategorSlotsDrawer _categorySlotsDrawer;
        [SerializeField] private Image _itemSelectedImage;
        [SerializeField] private UICraftingRequiredItems _requiredItems;
        [SerializeField] private UIImageWithLabel _itemSlotResultDrawer;
        [SerializeField] private UIItemSlotDragger _itemSlotResultDragger;

        private CraftSystem _craftSystem;
        private ItemDatabase _itemDatabase;
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
            _craftSystem = new CraftingTable(itemDatabase, InventoryType.CraftingTable);
            _itemDatabase = itemDatabase;
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
            ItemInfo creatingItem = _itemDatabase.Get(dragger.CurrentSlotContext.ItemStack.Item.Id);
            List<CraftVariant> availableVariants = _craftSystem.SelectAvailabilityVariants(_playerInventory, creatingItem.CraftVariants);
            _itemSelectedImage.sprite = creatingItem.Sprite;
            _requiredItems.SetUp(creatingItem, _itemDatabase, _craftSystem);

            if (availableVariants.Count > 0)
            {

            }
            else
            {
            }
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