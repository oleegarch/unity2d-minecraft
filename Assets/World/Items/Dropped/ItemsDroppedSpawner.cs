using UnityEngine;
using System.Collections.Generic;
using R3;
using World.Chunks;
using World.Inventories;
using System;

namespace World.Items
{
    public class ItemsDroppedSpawner : MonoBehaviour
    {
        [SerializeField] private WorldManager _manager;
        [SerializeField] private WorldEnvironmentAccessor _environment;
        [SerializeField] private GameObject _itemDroppedPrefab;
        [SerializeField] private Transform _itemsDroppedParent;

        private readonly List<IDisposable> _subscriptions = new();

        private void Start()
        {
            _subscriptions.Add(_manager.Events.BlockBroken.Subscribe(OnBlockBroken));
            _subscriptions.Add(_manager.Events.BlockInventoryRemoved.Subscribe(OnBlockInventoryRemoved));
        }

        private void OnDestroy()
        {
            foreach (var sub in _subscriptions) sub.Dispose();
            _subscriptions.Clear();
        }

        private void OnBlockBroken(WorldBlockEvent e)
        {
            var info = _environment.ItemDatabase.GetByBlockId(e.Block.Id);
            var stack = new ItemStack(info);
            DropItemAt(e.Position, stack);
        }

        private void OnBlockInventoryRemoved(WorldBlockInventoryEvent e)
        {
            foreach (var stack in e.Inventory.GetNonEmptySlots())
                DropItemAt(e.Position, stack);
        }

        public ItemDropped DropItemAt(WorldPosition worldPosition, ItemStack stack)
        {
            Vector3 pos = worldPosition.ToVector3Int();
            return DropItemAt(pos, stack);
        }

        public ItemDropped DropItemAt(Vector3 position, ItemStack stack)
        {
            if (stack.IsEmpty) return null;

            GameObject item = Instantiate(_itemDroppedPrefab, position, Quaternion.identity, _itemsDroppedParent);
            ItemDropped dropped = item.GetComponent<ItemDropped>();
            dropped.SetUp(stack, _environment.ItemDatabase);

            return dropped;
        }
    }
}