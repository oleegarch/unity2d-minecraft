using UnityEngine;
using World.Blocks;
using World.Chunks;
using World.HoveredBlock;
using World.Items;

namespace World.Inventories
{
    public class PlayerInventoryController : MonoBehaviour
    {
        [SerializeField] private UIPlayerHotbarDrawer _hotbar;
        [SerializeField] private HoveredBlockPicker _blockPicker;
        [SerializeField] private WorldManager _manager;
        private PlayerInventory _inventory;
        private int _hotbarActiveIndex = 0;

        public int ActiveHotbarIndex => _inventory.HotbarIndexToSlot(_hotbarActiveIndex);
        public ItemStack ActiveItemStack => _inventory.GetSlot(ActiveHotbarIndex);
        public ItemInfo ActiveItemInfo => ActiveItemStack.Item;

        private void Awake()
        {
            _inventory = new PlayerInventory();
            _hotbar.SetUp(_inventory);
            _blockPicker.OnBlockPickedChanged += HandleBlockPickedUpdate;
        }
        private void OnDestroy()
        {
            _blockPicker.OnBlockPickedChanged -= HandleBlockPickedUpdate;
        }
        private void HandleBlockPickedUpdate(WorldPosition position, Block block, BlockLayer blockLayer, BlockStyles blockStyles)
        {
            ItemInfo newItemInfo = _manager.ItemDatabase.GetByBlockId(block.Id);
            ItemStack newItemStack = new ItemStack(newItemInfo);
            _inventory.ReplaceSlot(ActiveHotbarIndex, newItemStack);
        }

        public bool TryCollect(ItemInfo item)
        {
            return _inventory.TryAdd(item);
        }
    }
}