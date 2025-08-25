using UnityEngine;
using World.Items;

namespace World.Entities.Player
{
    public class PlayerItemsCollector : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            ItemDropped itemDropped = collision.GetComponent<ItemDropped>();
            if (itemDropped == null || itemDropped.ItemInfo == null) return;

            Collect(itemDropped);
        }

        public void Collect(ItemDropped itemDropped)
        {
            itemDropped.Dispose();
        }
    }
}