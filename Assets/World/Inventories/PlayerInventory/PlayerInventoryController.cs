using System;
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
        [SerializeField] private HoveredBlockPicker _blockPicker;
        [SerializeField] private WorldManager _manager;
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private SpriteRenderer _itemOnRightHand;
        
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
            var actions = _inputManager.Controls.InventoryPlayerHotbar;
            actions.MouseScroll.performed += OnMouseScroll;
            actions.Enable();
        }
        private void OnDisable()
        {
            var actions = _inputManager.Controls.InventoryPlayerHotbar;
            actions.MouseScroll.performed -= OnMouseScroll;
            actions.Disable();
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

        private void OnMouseScroll(InputAction.CallbackContext context)
        {
            ActiveHotbarIndex += Math.Sign(context.ReadValue<float>());
        }

        public bool TryCollect(ItemInfo item)
        {
            return _inventory.TryAdd(item);
        }
    }
}