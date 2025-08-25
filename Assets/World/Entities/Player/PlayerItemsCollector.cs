using UnityEngine;
using World.Inventories;
using World.Items;

namespace World.Entities.Player
{
    public class PlayerItemsCollector : MonoBehaviour
    {
        [SerializeField] private PlayerInventoryController _inventoryController;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            ItemDropped itemDropped = collision.GetComponent<ItemDropped>();
            if (itemDropped == null || itemDropped.ItemInfo == null) return;

            Collect(itemDropped);
        }

        public void Collect(ItemDropped itemDropped)
        {
            ItemInfo item = itemDropped.ItemInfo;

            if (_inventoryController.TryCollect(item))
                itemDropped.Dispose();
        }
    }
}