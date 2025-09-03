using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using World.Chunks;
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
        [SerializeField] private UIItemSlotDrawer _itemSlotResultDrawer;
        [SerializeField] private UIItemSlotDragger _itemSlotResultDragger;

        private WorldManager _manager;
        private CraftSystem _craftSystem;
        private ItemDatabase _itemDatabase;
        private PlayerInventory _playerInventory;
        private BlockInventory _blockInventory;
        private CraftVariant _craftingVariant;
        private int _craftSlotIndex;

        private void OnEnable()
        {
            _categorySlotsDrawer.OnSlotCreated += OnSlotCreated;
            _categorySlotsDrawer.OnSlotDestroy += OnSlotDestroy;
            _itemSlotResultDragger.OnBeforeDrop += OnCraftBeforeDrop;
        }
        private void OnDisable()
        {
            _categorySlotsDrawer.OnSlotCreated -= OnSlotCreated;
            _categorySlotsDrawer.OnSlotDestroy -= OnSlotDestroy;
            _itemSlotResultDragger.OnBeforeDrop -= OnCraftBeforeDrop;
        }

        public void SetUp(PlayerInventory inventory, BlockInventory blockInventory, WorldManager manager)
        {
            _manager = manager;
            _itemDatabase = manager.ItemDatabase;
            _craftSystem = new CraftingTable(_itemDatabase, InventoryType.CraftingTable);
            _blockInventory = blockInventory;
            _playerInventory = inventory;
            _categorySlotsDrawer.SetUp(_itemDatabase);
            _categoriesDrawer.SetUp(manager.ItemCategoryDatabase);
            _craftSlotIndex = 0;
            
            ItemStack craftStack = blockInventory.GetSlot(_craftSlotIndex);
            _itemSlotResultDrawer.SetUp(craftStack, _itemDatabase);
            _itemSlotResultDragger.SetSlotContext(new SlotContext(_blockInventory, _craftSlotIndex));
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
            if (_blockInventory.HasEmptySlot)
            {
                ItemInfo selectedItem = _itemDatabase.Get(dragger.CurrentSlotContext.ItemStack.Item.Id);
                List<CraftVariant> availableVariants = _craftSystem.SelectAvailabilityVariants(_playerInventory, selectedItem.CraftVariants);
                _itemSelectedImage.sprite = selectedItem.Sprite;
                _requiredItems.SetUp(selectedItem, _itemDatabase, _craftSystem);

                if (availableVariants.Count > 0)
                {
                    CraftVariant availableVariant = availableVariants[0];
                    ItemStack previewStack = new ItemStack(selectedItem, availableVariant.ReturnCount);
                    _itemSlotResultDrawer.Refresh(previewStack);
                    _itemSlotResultDrawer.SetColor(Color.green);
                    _itemSlotResultDragger.SetSlotContext(new SlotContext(previewStack, SlotType.Preview));
                    _craftingVariant = availableVariant;
                    Debug.Log($"OnSlotClicked green: {_itemSlotResultDragger.CurrentSlotContext.ItemStack}");
                }
                else
                {
                    ItemStack previewStack = ItemStack.Empty;
                    _itemSlotResultDrawer.Refresh(previewStack);
                    _itemSlotResultDrawer.SetColor(Color.red);
                    _itemSlotResultDragger.SetSlotContext(new SlotContext(previewStack, SlotType.Preview));
                    Debug.Log($"OnSlotClicked red: {_itemSlotResultDragger.CurrentSlotContext.ItemStack}");
                }
            }
        }
        public void OnCraftBeforeDrop(UIItemSlotDragger fromDragger, UIItemSlotDragger toDragger)
        {
            Debug.Log($"OnCraftBeforeDrop: {fromDragger.name}, {fromDragger.CurrentSlotContext.ItemStack}");
            SlotContext fromContext = fromDragger.CurrentSlotContext;
            ItemInfo craftingItem = _itemDatabase.Get(fromContext.ItemStack.Item.Id);
            _craftSystem.Craft(_playerInventory, _blockInventory, craftingItem, _craftingVariant.Id);
            fromDragger.SetSlotContext(new SlotContext(_blockInventory, _craftSlotIndex));
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