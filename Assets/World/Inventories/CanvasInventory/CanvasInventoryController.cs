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
        [SerializeField] private UIPlayerHotbarDrawer _hotbar;
        [SerializeField] private UIPlayerMainSlotsDrawer _mainSlots;
        [SerializeField] private GameObject _slotsInventoryPrefab;
        [SerializeField] private Transform _inventoryAlignmentTransform;
        [SerializeField] private Transform _itemOnRightHandTransform;
        [SerializeField] private ItemsDroppedSpawner _itemsSpawner;
        [SerializeField] private HoveredBlockObserver _blockObserver;
        [SerializeField] private HoveredBlockPicker _blockPicker;
        [SerializeField] private WorldManager _manager;
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private SpriteRenderer _itemOnRightHand;
        [SerializeField] private UIMask _uiMask;
        
        private PlayerInventory _inventory;
        private UIInventorySlotsDrawer _foreignDrawer;
        private int _hotbarActiveIndex;

        public int ActiveHotbarIndex
        {
            get
            {
                return _inventory.HotbarIndexToSlot(_hotbarActiveIndex);
            }
            set
            {
                _hotbarActiveIndex = _hotbar.ChangeActiveHotbar(value);
                ChangeActiveItemInfoOnRightHand();
            }
        }
        public ItemStack ActiveItemStack => _inventory.GetSlot(ActiveHotbarIndex);
        public ItemInfo ActiveItemInfo => ActiveItemStack.Item?.GetItemInfo(_manager.ItemDatabase);
        public PlayerInventory Inventory => _inventory;

        private void Awake()
        {
            _inventory = new PlayerInventory();
            _hotbar.SetUp(_inventory);
            _mainSlots.SetUp(_inventory);
            _blockPicker.OnBlockPickedChanged += HandleBlockPickedUpdate;
            _inventory.Events.SlotChanged += HandleInventorySlotChanged;
        }
        private void Start()
        {
            _inventory.TryAdd(new ItemStack(_manager.ItemDatabase.GetByBlockId(31), 100), out int remainder1);
            _inventory.TryAdd(new ItemStack(_manager.ItemDatabase.GetByBlockId(33), 100), out int remainder2);
            ActiveHotbarIndex = 0;
        }
        private void OnDestroy()
        {
            _blockPicker.OnBlockPickedChanged -= HandleBlockPickedUpdate;
            _inventory.Events.SlotChanged -= HandleInventorySlotChanged;
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
            _inventory.Replace(ActiveHotbarIndex, newItemStack, out ItemStack old);
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
                _uiMask.Appear();
            else
            {
                _foreignDrawer?.Dispose();
                _foreignDrawer = null;
                _uiMask.Disappear();
            }
        }
        private void OpenInventory()
        {
            _mainSlots.Open();
            _uiMask.Appear();
        }
        private void OpenForeignInventory(InputAction.CallbackContext context)
        {
            if (
                !_blockObserver.ReachedLimitPosition &&
                _manager.Blocks.TryGetInventory(_blockObserver.HoveredPosition, out Inventory inventory)
            )
            {
                _foreignDrawer?.Dispose();
                _foreignDrawer = null;
                _foreignDrawer = Instantiate(_slotsInventoryPrefab, _inventoryAlignmentTransform).GetComponent<UIInventorySlotsDrawer>();
                _foreignDrawer.SetUp(_manager, inventory);
                _foreignDrawer.Open();

                OpenInventory();
            }
        }
        private void DropCurrentItem(InputAction.CallbackContext context)
        {
            if (_inventory.TryRemove(ActiveHotbarIndex, 1, out ItemStack removed) && !removed.IsEmpty)
            {
                for (int i = 0; i < removed.Quantity; i++)
                {
                    ItemDropped dropped = _itemsSpawner.DropItemAt(_itemOnRightHandTransform.position, removed);
                    dropped.ThrowItem(_blockObserver.CursorPositionInChunks);
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
            return _inventory.TryAdd(stack, out int remainder);
        }
    }
}