using UnityEngine;

namespace World.Inventories
{
    public class PlayerInventoryController : MonoBehaviour
    {
        [SerializeField] private UIPlayerHotbarDrawer _hotbar;
        private PlayerInventory _inventory;

        private void Awake()
        {
            _inventory = new PlayerInventory();
            _hotbar.SetUp(_inventory);
        }
    }
}