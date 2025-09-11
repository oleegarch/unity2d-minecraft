using System;
using UIGlobal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using World.Blocks;
using World.Chunks;
using World.HoveredBlock;
using World.InputActions;
using World.Items;

namespace World.Inventories
{
    public class CanvasInventoryController : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private UIMask _uiMask;

        [Header("Inventory references")]
        [SerializeField] private UIPlayerHotbarDrawer _hotbar;
        [SerializeField] private UIPlayerMainSlotsDrawer _mainSlots;
        [SerializeField] private GameObject _creativeInventoryPrefab;
        [SerializeField] private GameObject _craftingInventoryPrefab;
        [SerializeField] private GameObject _slotsInventoryPrefab;
        [SerializeField] private Transform _inventoryAlignmentTransform;
        
        [Header("World systems")]
        [SerializeField] private WorldModeController _worldMode;
        [SerializeField] private WorldManager _manager;
        [SerializeField] private WorldInputManager _inputManager;

        [Header("Player right hand")]
        [SerializeField] private Animator _localPlayerAnimator;
        [SerializeField] private Transform _itemOnRightHandTransform;
        [SerializeField] private SpriteRenderer _itemOnRightHand;

        [Header("Other")]
        [SerializeField] private ItemsDroppedSpawner _itemsSpawner;
        [SerializeField] private HoveredBlockObserver _blockObserver;
        [SerializeField] private HoveredBlockPicker _blockPicker;

        private PlayerInventory _playerInventory;
        private IUIInventoryAccessor _foreignInventoryAccessor;
        private int _hotbarActiveIndex;

        public int ActiveHotbarIndex
        {
            get
            {
                return _playerInventory.HotbarIndexToSlot(_hotbarActiveIndex);
            }
            set
            {
                _hotbarActiveIndex = _hotbar.ChangeActiveHotbar(value);
                ChangeActiveItemInfoOnRightHand();
            }
        }
        public ItemStack ActiveItemStack => _playerInventory.GetSlot(ActiveHotbarIndex);
        public ItemInfo ActiveItemInfo => ActiveItemStack.Item?.GetItemInfo(_manager.ItemDatabase);
        public PlayerInventory Inventory => _playerInventory;

        private void Awake()
        {
            _playerInventory = new PlayerInventory();
            _hotbar.SetUp(_playerInventory);
            _mainSlots.SetUp(_playerInventory);
            _blockPicker.OnBlockPickedChanged += HandleBlockPickedUpdate;
            _playerInventory.Events.SlotChanged += HandleInventorySlotChanged;
        }
        private void Start()
        {
            ActiveHotbarIndex = 0;
        }
        private void OnDestroy()
        {
            _blockPicker.OnBlockPickedChanged -= HandleBlockPickedUpdate;
            _playerInventory.Events.SlotChanged -= HandleInventorySlotChanged;
            _hotbar.Dispose();
        }
        private void OnEnable()
        {
            var inventory = _inputManager.Controls.InventoryPlayer;
            inventory.Toggle.performed += ToggleInventory;
            inventory.Drop.performed += DropCurrentItem;
            inventory.OpenForeign.performed += OpenForeignInventory;
            inventory.Enable();
            var hotbar = _inputManager.Controls.InventoryPlayerHotbar;
            hotbar.MouseScroll.performed += OnMouseScroll;
            hotbar.Digit.performed += OnDigitPressed;
            hotbar.Enable();
        }
        private void OnDisable()
        {
            var inventory = _inputManager.Controls.InventoryPlayer;
            inventory.Toggle.performed -= ToggleInventory;
            inventory.Drop.performed -= DropCurrentItem;
            inventory.OpenForeign.performed -= OpenForeignInventory;
            inventory.Disable();
            var hotbar = _inputManager.Controls.InventoryPlayerHotbar;
            hotbar.MouseScroll.performed -= OnMouseScroll;
            hotbar.Digit.performed -= OnDigitPressed;
            hotbar.Disable();
        }
        private void HandleBlockPickedUpdate(WorldPosition position, Block block, BlockLayer blockLayer, BlockStyles blockStyles)
        {
            ItemInfo newItemInfo = _manager.ItemDatabase.GetByBlockId(block.Id);
            ItemStack newItemStack = new ItemStack(newItemInfo, quantity: newItemInfo.MaxStack);
            _playerInventory.Replace(ActiveHotbarIndex, newItemStack, out ItemStack old);
        }
        private void HandleInventorySlotChanged(object sender, SlotChangedEventArgs args)
        {
            if (args.SlotIndex == ActiveHotbarIndex)
                ChangeActiveItemInfoOnRightHand();
        }
        private void ChangeActiveItemInfoOnRightHand()
        {
            _itemOnRightHand.enabled = ActiveItemInfo != null && ActiveItemInfo.Sprite != null;

            if (_itemOnRightHand.enabled)
                _itemOnRightHand.sprite = ActiveItemInfo.Sprite;
        }

        private void ToggleInventory(InputAction.CallbackContext context)
        {
            if (_mainSlots.Toggle())
            {
                _uiMask.Appear();

                if (_worldMode.WorldMode == WorldMode.Creative)
                    ChangeForeignInventory(CreateCreativeInventory());
            }
            else
            {
                _foreignInventoryAccessor?.Dispose();
                _foreignInventoryAccessor = null;
                _uiMask.Disappear();
            }
        }
        private UIInventorySlotsDrawer CreateForeignInventory(Inventory inventory)
        {
            GameObject go = Instantiate(_slotsInventoryPrefab, _inventoryAlignmentTransform);
            UIInventorySlotsDrawer component = go.GetComponent<UIInventorySlotsDrawer>();
            component.SetUp(_manager, inventory);
            return component;
        }
        private UICreativeInventory CreateCreativeInventory()
        {
            GameObject go = Instantiate(_creativeInventoryPrefab, _inventoryAlignmentTransform);
            UICreativeInventory component = go.GetComponent<UICreativeInventory>();
            component.SetUp(_manager.ItemDatabase, _manager.ItemCategoryDatabase);
            return component;
        }
        private UICraftingInventory CreateCraftingInventory(BlockInventory tableInventory)
        {
            GameObject go = Instantiate(_craftingInventoryPrefab, _inventoryAlignmentTransform);
            UICraftingInventory component = go.GetComponent<UICraftingInventory>();
            component.SetUp(_playerInventory, tableInventory, _manager);
            return component;
        }
        private void OpenForeignInventory(InputAction.CallbackContext context)
        {
            if (!_blockObserver.ReachedLimitPosition)
            {
                Block clickedBlock = _manager.Blocks.GetBreakable(_blockObserver.HoveredPosition, out BlockLayer blockLayer);
                if (!clickedBlock.IsAir)
                {
                    BlockInfo clickedBlockInfo = _manager.BlockDatabase.Get(clickedBlock.Id);
                        
                    if (clickedBlockInfo.Inventory.Type == InventoryType.Inventory)
                    {
                        if (_manager.Blocks.TryGetInventory(_blockObserver.HoveredPosition, out BlockInventory inventory))
                        {
                            ChangeForeignInventory(CreateForeignInventory(inventory));
                            return;
                        }
                    }
                    else if (clickedBlockInfo.Inventory.Type == InventoryType.CraftingTable)
                    {
                        if (_manager.Blocks.TryGetInventory(_blockObserver.HoveredPosition, out BlockInventory tableInventory))
                        {
                            ChangeForeignInventory(CreateCraftingInventory(tableInventory));
                            return;
                        }
                    }
                }
            }
        }
        private void ChangeForeignInventory(IUIInventoryAccessor newForeignInventory)
        {
            _foreignInventoryAccessor?.Dispose();
            _foreignInventoryAccessor = newForeignInventory;

            if (_foreignInventoryAccessor != null)
            {
                _foreignInventoryAccessor.Open();
                _mainSlots.Open();
                _uiMask.Appear();
            }
        }
        private void DropCurrentItem(InputAction.CallbackContext context)
        {
            if (_playerInventory.TryRemove(ActiveHotbarIndex, 1, out ItemStack removed) && !removed.IsEmpty)
            {
                for (int i = 0; i < removed.Quantity; i++)
                {
                    ItemDropped dropped = _itemsSpawner.DropItemAt(_itemOnRightHandTransform.position, removed);
                    dropped.ThrowItem(_blockObserver.CursorPositionInChunks);
                    _localPlayerAnimator.SetTrigger("Throw");
                }
            }
        }
        private void OnMouseScroll(InputAction.CallbackContext context)
        {
            ActiveHotbarIndex += Math.Sign(context.ReadValue<float>()) * -1; // reverse
        }
        private void OnDigitPressed(InputAction.CallbackContext context)
        {
            var control = context.control;

            if (control is KeyControl keyControl)
            {
                Key key = keyControl.keyCode;
                int hotbarIndex = -1;
                switch (key)
                {
                    case Key.Digit1: hotbarIndex = 0; break;
                    case Key.Digit2: hotbarIndex = 1; break;
                    case Key.Digit3: hotbarIndex = 2; break;
                    case Key.Digit4: hotbarIndex = 3; break;
                    case Key.Digit5: hotbarIndex = 4; break;
                    case Key.Digit6: hotbarIndex = 5; break;
                    case Key.Digit7: hotbarIndex = 6; break;
                    case Key.Digit8: hotbarIndex = 7; break;
                    case Key.Digit9: hotbarIndex = 8; break;
                    case Key.Digit0: hotbarIndex = 9; break;
                }

                if (hotbarIndex != -1)
                    ActiveHotbarIndex = hotbarIndex;
            }
        }

        public bool TryCollect(ItemStack stack)
        {
            return _playerInventory.TryAdd(stack);
        }
    }
}