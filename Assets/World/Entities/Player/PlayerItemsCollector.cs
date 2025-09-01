using UnityEngine;
using World.Inventories;
using World.Items;

namespace World.Entities.Player
{
    public class PlayerItemsCollector : MonoBehaviour
    {
        [SerializeField] private CanvasInventoryController _inventoryController;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            ItemDropped itemDropped = collision.GetComponent<ItemDropped>();
            if (itemDropped == null || itemDropped.Stack == null) return;

            Collect(itemDropped);
        }

        public void Collect(ItemDropped itemDropped)
        {
            ItemStack stack = itemDropped.Stack;

            if (_inventoryController.TryCollect(stack))
                itemDropped.Dispose();
        }
    }
}