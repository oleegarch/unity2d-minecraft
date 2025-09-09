using System.Collections.Generic;
using System.Linq;
using TMPro;
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
        [SerializeField] private TextMeshProUGUI _requiredTitle;
        [SerializeField] private TextMeshProUGUI _selectItemTitle;
        [SerializeField] private UICraftingRequiredItems _requiredItems;
        [SerializeField] private UIItemSlotDrawer _itemSlotResultDrawer;
        [SerializeField] private UIItemSlotDragger _itemSlotResultDragger;

        private WorldManager _manager;
        private CraftSystem _craftSystem;
        private ItemDatabase _itemDatabase;
        private ItemCategoryDatabase _itemCategoryDatabase;
        private PlayerInventory _playerInventory;
        private BlockInventory _blockInventory;
        private CraftVariant _craftingVariant;
        private int _craftSlotIndex;
        private ushort _selectedToCraftItemId;

        private ItemStack _blockInveontorySlot => _blockInventory.GetSlot(_craftSlotIndex);
        private ItemInfo _selectedToCraftItem => _itemDatabase.Get(_selectedToCraftItemId);
        private List<CraftVariant> _availableVariants => _craftSystem.SelectAvailabilityVariants(_playerInventory, _selectedToCraftItem.CraftVariants);
        private bool _requireCraft => _selectedToCraftItemId != 0 && (_blockInveontorySlot.IsEmpty || _blockInveontorySlot.Item.Id == _selectedToCraftItemId);
        
        private IEnumerable<ItemInfo> _availableItems
            => _itemDatabase.items
                .Where(info => info.CraftVariants.IsAvailableFor(_craftSystem.InventoryType));

        private HashSet<ItemCategory> _availableCategories 
            => _availableItems
                .Select(info => info.Category)
                .ToHashSet();

        private IEnumerable<ItemCategoryInfo> _availableCategoryInfos 
            => _availableCategories
                .Select(category => _itemCategoryDatabase.Get(category));


        private void Awake()
        {
            _selectItemTitle.enabled = true;
            _itemSelectedImage.enabled = false;
            _requiredTitle.enabled = false;
            _requiredItems.gameObject.SetActive(false);
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
            _itemCategoryDatabase = manager.ItemCategoryDatabase;
            _craftSystem = new CraftingTable(_itemDatabase, InventoryType.CraftingTable);
            _blockInventory = blockInventory;
            _playerInventory = inventory;
            _categorySlotsDrawer.SetUp(_availableItems);
            _categoriesDrawer.SetUp(_availableCategoryInfos);
            _craftSlotIndex = 0;
            ConfigureCraftSlot();
            RefreshCraftSlot();
        }

        public void OnSlotCreated(GameObject go, ItemInfo info)
        {
            var stack = new ItemStack(info);
            var drawer = go.GetComponent<UIItemSlotDrawer>();
            var dragger = go.GetComponent<UIItemSlotDragger>();
            drawer.SetUpStack(info.Sprite, stack.Quantity.ToString());
            drawer.ToggleCountLabel(false);
            dragger.SetSlotContext(new SlotContext(stack, SlotType.Preview));
            dragger.OnClick += OnSlotClicked;
        }
        public void OnSlotDestroy(GameObject go)
        {
            var dragger = go.GetComponent<UIItemSlotDragger>();
            dragger.OnClick -= OnSlotClicked;
        }
        public void OnSlotClicked(UIItemSlotDrawer drawer, UIItemSlotDragger dragger)
        {
            _selectedToCraftItemId = dragger.CurrentSlotContext.ItemStack.Item.Id;

            SelectItemToCraft();
            RefreshCraftSlot();
        }
        public void OnCraftSlotClicked(UIItemSlotDrawer drawer, UIItemSlotDragger dragger)
        {
            if (_requireCraft)
            {
                CraftItem();
                RefreshCraftSlot();
            }
        }
        public void OnCraftSlotBeforeDrop(UIItemSlotDragger fromDragger, UIItemSlotDragger toDragger)
        {
            if (_requireCraft && !UIItemSlotDragger.DraggingByClick)
            {
                CraftItem();
            }
        }
        public void OnCraftSlotDropped(UIItemSlotDragger fromDragger, UIItemSlotDragger toDragger)
        {
            RefreshCraftSlot();
        }

        public void SelectItemToCraft()
        {
            _selectItemTitle.enabled = false;
            _requiredTitle.enabled = true;
            _itemSelectedImage.enabled = true;
            _itemSelectedImage.sprite = _selectedToCraftItem.Sprite;
            _requiredItems.gameObject.SetActive(true);
            _requiredItems.SetUp(_selectedToCraftItem, _itemDatabase, _craftSystem);
        }
        public void CraftItem()
        {
            if (_craftingVariant != null)
            {
                _craftSystem.Craft(_playerInventory, _blockInventory, _selectedToCraftItem, _craftingVariant.Id);
            }
        }
        public void ConfigureCraftSlot()
        {
            _itemSlotResultDragger.SetSlotContextCustom(new SlotContext(_blockInventory, _craftSlotIndex));
            _itemSlotResultDragger.ToggleDragHandlers(true);
            _itemSlotResultDragger.ToggleDragging(true);
            _itemSlotResultDragger.ToggleDrop(false);
        }
        public void RefreshCraftSlot()
        {
            if (_requireCraft)
            {
                bool canCraft = _availableVariants.Count > 0;

                _itemSlotResultDrawer.SetColor(canCraft ? Color.green : Color.red);
                _itemSlotResultDragger.ToggleModifyingSourceStack(false);
                _itemSlotResultDragger.ToggleRightClick(false);
                _itemSlotResultDragger.UpdateDraggingStack(_selectedToCraftItem.Sprite, _blockInveontorySlot.Quantity);
                _craftingVariant = canCraft ? _availableVariants[0] : null;

                if (canCraft)
                {
                    CraftVariant availableVariant = _availableVariants[0];
                    ItemStack previewStack = new ItemStack(_selectedToCraftItem, availableVariant.ReturnCount);
                    _itemSlotResultDrawer.SetUpStack(_itemDatabase, previewStack);
                }
                else if (!UIItemSlotDragger.DraggingStarted)
                {
                    _itemSlotResultDrawer.SetUpStack(_itemDatabase, _blockInveontorySlot);
                }
                else
                {
                    _itemSlotResultDrawer.DisableStack();
                }
            }
            else
            {
                _itemSlotResultDrawer.ClearColor();
                _itemSlotResultDrawer.SetUpStack(_itemDatabase, _blockInveontorySlot);
                _itemSlotResultDragger.ToggleModifyingSourceStack(true);
                _itemSlotResultDragger.ToggleRightClick(true);
                _craftingVariant = null;
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