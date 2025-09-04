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
        private ushort _selectedToCraftItemId;
        private bool _likeInventory
        {
            get
            {
                var slot = _blockInventory.GetSlot(_craftSlotIndex);
                if (slot.IsEmpty) return false;
                return slot.Item.Id != _selectedToCraftItemId;
            }
        }

        private void OnEnable()
        {
            _categorySlotsDrawer.OnSlotCreated += OnSlotCreated;
            _categorySlotsDrawer.OnSlotDestroy += OnSlotDestroy;
            _itemSlotResultDragger.OnClick += OnCraftSlotClicked;
            _itemSlotResultDragger.OnBeforeDrop += OnCraftSlotBeforeDrop;
            _itemSlotResultDragger.OnDropped += OnCraftSlotDropped;
        }
        private void OnDisable()
        {
            _categorySlotsDrawer.OnSlotCreated -= OnSlotCreated;
            _categorySlotsDrawer.OnSlotDestroy -= OnSlotDestroy;
            _itemSlotResultDragger.OnClick -= OnCraftSlotClicked;
            _itemSlotResultDragger.OnBeforeDrop -= OnCraftSlotBeforeDrop;
            _itemSlotResultDragger.OnDropped -= OnCraftSlotDropped;
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
            _itemSlotResultDragger.ToggleHidingSourceStack(false);
            _craftSlotIndex = 0;

            _itemSlotResultDrawer.SetUpStack(_itemDatabase, blockInventory.GetSlot(_craftSlotIndex));
            _itemSlotResultDragger.DisableDrop();

            if (!_blockInventory.HasEmptySlot)
            {
                // _itemSlotResultDragger.ToggleHidingSourceStack(true);
                SetCraftSlotLikeInventorySlot();
            }
        }

        public void OnSlotCreated(GameObject go, ItemInfo info)
        {
            var stack = new ItemStack(info);
            var drawer = go.GetComponent<UIItemSlotDrawer>();
            var dragger = go.GetComponent<UIItemSlotDragger>();
            drawer.SetUpStack(info.Sprite, stack.Quantity.ToString());
            drawer.ToggleCountLabel(false);
            dragger.SetSlotContext(new SlotContext(stack, SlotType.Preview));
            dragger.DisableDragging();
            dragger.OnClick += OnSlotClicked;
        }
        public void OnSlotDestroy(GameObject go)
        {
            var dragger = go.GetComponent<UIItemSlotDragger>();
            dragger.OnClick -= OnSlotClicked;
        }
        public void OnSlotClicked(UIItemSlotDrawer drawer, UIItemSlotDragger dragger)
        {
            if (_likeInventory || !_blockInventory.HasEmptySlot) return;

            _selectedToCraftItemId = dragger.CurrentSlotContext.ItemStack.Item.Id;
            SelectItemToCraft(_selectedToCraftItemId);
            SetCraftSlotLikeCraftingSlot(_selectedToCraftItemId);
            ShowCraftAvailability(_selectedToCraftItemId);
            CraftSlotToggleCount();
        }
        public void OnCraftSlotClicked(UIItemSlotDrawer drawer, UIItemSlotDragger dragger)
        {
            if (_likeInventory) return;

            if (_craftingVariant != null)
            {
                CraftItem(_selectedToCraftItemId);
            }

            ShowCraftAvailability(_selectedToCraftItemId);
            CraftSlotToggleCount();
        }
        public void OnCraftSlotBeforeDrop(UIItemSlotDragger fromDragger, UIItemSlotDragger toDragger)
        {
            if (_likeInventory || _craftingVariant == null || UIItemSlotDragger.DraggingByClick) return;

            CraftItem(_selectedToCraftItemId);
        }
        public void OnCraftSlotDropped(UIItemSlotDragger fromDragger, UIItemSlotDragger toDragger)
        {
            if (!_likeInventory)
            {
                SetCraftSlotLikeCraftingSlot(_selectedToCraftItemId);
                ShowCraftAvailability(_selectedToCraftItemId);
                CraftSlotToggleCount();
            }
        }

        public void SelectItemToCraft(ushort itemId)
        {
            ItemInfo selectedItem = _itemDatabase.Get(itemId);
            _itemSelectedImage.sprite = selectedItem.Sprite;
            _requiredItems.SetUp(selectedItem, _itemDatabase, _craftSystem);
        }
        public void CraftSlotToggleCount()
        {
            _itemSlotResultDrawer.ToggleCountLabel(_blockInventory.HasEmptySlot || _likeInventory);
        }
        public void CraftItem(ushort itemId)
        {
            ItemInfo craftingItem = _itemDatabase.Get(itemId);
            if (_craftSystem.Craft(_playerInventory, _blockInventory, craftingItem, _craftingVariant.Id))
                SetCraftSlotLikeInventorySlot();
        }
        public void ShowCraftAvailability(ushort itemId)
        {
            ItemInfo selectedItem = _itemDatabase.Get(itemId);
            List<CraftVariant> availableVariants = _craftSystem.SelectAvailabilityVariants(_playerInventory, selectedItem.CraftVariants);

            if (availableVariants.Count > 0)
            {
                CraftVariant availableVariant = availableVariants[0];
                ItemStack previewStack = new ItemStack(selectedItem, availableVariant.ReturnCount);
                _itemSlotResultDrawer.SetUpStack(_itemDatabase, previewStack);
                _itemSlotResultDrawer.SetColor(Color.green);
            }
            else
            {
                _itemSlotResultDrawer.SetUpStack(_itemDatabase, ItemStack.Empty);
                _itemSlotResultDrawer.SetColor(Color.red);
            }
        }
        public void SetCraftSlotLikeCraftingSlot(ushort itemId)
        {
            ItemInfo selectedItem = _itemDatabase.Get(itemId);
            List<CraftVariant> availableVariants = _craftSystem.SelectAvailabilityVariants(_playerInventory, selectedItem.CraftVariants);

            if (availableVariants.Count > 0)
            {
                CraftVariant availableVariant = availableVariants[0];
                ItemStack previewStack = new ItemStack(selectedItem, availableVariant.ReturnCount);
                _itemSlotResultDragger.SetSlotContext(new SlotContext(previewStack, SlotType.Preview));
                _craftingVariant = availableVariant;
            }
            else
            {
                _itemSlotResultDragger.SetSlotContext(new SlotContext(ItemStack.Empty, SlotType.Preview));
            }
        }
        public void SetCraftSlotLikeInventorySlot()
        {
            ItemStack craftStack = _blockInventory.GetSlot(_craftSlotIndex);
            _itemSlotResultDrawer.SetUpStack(_itemDatabase, craftStack);
            _itemSlotResultDragger.SetSlotContext(new SlotContext(_blockInventory, _craftSlotIndex));

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