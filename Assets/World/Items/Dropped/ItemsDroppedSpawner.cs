using UnityEngine;
using World.Blocks;
using World.Chunks;

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
        }
        private void OnDestroy()
        {
            _manager.Blocks.Events.OnBlockBroken -= HandleWorldBlockBroken;
        }

        private void HandleWorldBlockBroken(WorldPosition position, Block block, BlockLayer blockLayer)
        {
            ItemInfo info = _manager.ItemDatabase.GetByBlockId(block.Id);
            DropItemAt(position, info);
        }
        public ItemDropped DropItemAt(WorldPosition worldPosition, ItemInfo itemInfo)
        {
            Vector3 position = worldPosition.ToVector3Int();
            return DropItemAt(position, itemInfo);
        }
        public ItemDropped DropItemAt(Vector3 position, ItemInfo itemInfo)
        {
            GameObject item = Instantiate(_itemDroppedPrefab, position, Quaternion.identity, _itemsDroppedParent);
            ItemDropped dropped = item.GetComponent<ItemDropped>();
            dropped.SetUp(itemInfo);

            return dropped;
        }
    }
}