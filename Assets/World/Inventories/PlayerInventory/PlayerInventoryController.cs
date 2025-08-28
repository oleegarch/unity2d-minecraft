using System;
using UIGlobal;
using UnityEngine;
using UnityEngine.InputSystem;
using World.Blocks;
using World.Chunks;
using World.HoveredBlock;
using World.InputActions;
using World.Items;

namespace World.Inventories
{
    public class PlayerInventoryController : MonoBehaviour
    {
        [SerializeField] private UIPlayerHotbarDrawer _hotbar;
        [SerializeField] private UIPlayerMainSlotsDrawer _mainSlots;
        [SerializeField] private UIInventorySlotsDrawer _foreignSlots;
        [SerializeField] private Transform _itemOnRightHandTransform;
        [SerializeField] private ItemsDroppedSpawner _itemsSpawner;
        [SerializeField] private HoveredBlockObserver _blockObserver;
        [SerializeField] private HoveredBlockPicker _blockPicker;
        [SerializeField] private WorldManager _manager;
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private SpriteRenderer _itemOnRightHand;
        [SerializeField] private UIMask _uiMask;
        
        private PlayerInventory _inventory;
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
        public ItemInfo ActiveItemInfo => ActiveItemStack.Item;

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
            hotbar.Disable();
        }
        private void HandleBlockPickedUpdate(WorldPosition position, Block block, BlockLayer blockLayer, BlockStyles blockStyles)
        {
            ItemInfo newItemInfo = _manager.ItemDatabase.GetByBlockId(block.Id);
            ItemStack newItemStack = new ItemStack(newItemInfo);
            _inventory.ReplaceSlot(ActiveHotbarIndex, newItemStack);
        }
        private void HandleInventorySlotChanged(int slotIndex, ItemStack newStack)
        {
            if (slotIndex == ActiveHotbarIndex)
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
                _foreignSlots.Close();
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
            if (_manager.Blocks.TryGetInventory(_blockObserver.HoveredPosition, out Inventory inventory))
            {
                _foreignSlots.SetUp(inventory);
                _foreignSlots.Open();
                OpenInventory();
            }
        }
        private void DropCurrentItem(InputAction.CallbackContext context)
        {
            if (_inventory.TryRemove(ActiveHotbarIndex, 1, out ItemStack removed))
            {
                for (int i = 0; i < removed.Count; i++)
                {
                    ItemDropped dropped = _itemsSpawner.DropItemAt(_itemOnRightHandTransform.position, removed);
                    dropped.ThrowItem(_blockObserver.CursorPosition);
                }
            }
        }
        private void OnMouseScroll(InputAction.CallbackContext context)
        {
            ActiveHotbarIndex += Math.Sign(context.ReadValue<float>()) * -1; // reverse
        }

        public bool TryCollect(ItemStack stack)
        {
            return _inventory.TryAdd(stack, out int remainder);
        }
    }
}