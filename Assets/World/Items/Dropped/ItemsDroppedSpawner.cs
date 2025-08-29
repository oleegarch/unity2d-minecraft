using UnityEngine;
using World.Blocks;
using World.Chunks;
using World.Inventories;

namespace World.Items
{
    public class ItemsDroppedSpawner : MonoBehaviour
    {
        [SerializeField] private WorldManager _manager;
        [SerializeField] private GameObject _itemDroppedPrefab;
        [SerializeField] private Transform _itemsDroppedParent;

        private void Start()
        {
            _manager.Blocks.Events.OnBlockBroken += HandleWorldBlockBroken;
            _manager.Blocks.Events.OnBlockInventoryDropped += HandleWorldBlockInventoryDropped;
        }
        private void OnDestroy()
        {
            _manager.Blocks.Events.OnBlockBroken -= HandleWorldBlockBroken;
            _manager.Blocks.Events.OnBlockInventoryDropped -= HandleWorldBlockInventoryDropped;
        }

        private void HandleWorldBlockBroken(WorldPosition position, Block block, BlockLayer blockLayer)
        {
            ItemInfo info = _manager.ItemDatabase.GetByBlockId(block.Id);
            ItemStack stack = new ItemStack(info, 1);
            DropItemAt(position, stack);
        }
        private void HandleWorldBlockInventoryDropped(WorldPosition position, Inventory inventory, BlockLayer layer)
        {
            foreach (var stack in inventory.GetAllNotEmptySlots())
                DropItemAt(position, stack);
        }
        public ItemDropped DropItemAt(WorldPosition worldPosition, ItemStack stack)
        {
            Vector3 position = worldPosition.ToVector3Int();
            return DropItemAt(position, stack);
        }
        public ItemDropped DropItemAt(Vector3 position, ItemStack stack)
        {
            if (stack.IsEmpty) return null;

            GameObject item = Instantiate(_itemDroppedPrefab, position, Quaternion.identity, _itemsDroppedParent);
            ItemDropped dropped = item.GetComponent<ItemDropped>();
            dropped.SetUp(stack);

            return dropped;
        }
    }
}