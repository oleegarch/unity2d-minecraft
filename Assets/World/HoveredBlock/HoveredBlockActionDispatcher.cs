using UnityEngine;
using World.Chunks;
using World.Entities;
using World.Inventories;
using World.Items;

namespace World.HoveredBlock
{
    public class HoveredBlockActionDispatcher : MonoBehaviour
    {
        [SerializeField] private WorldEntities _worldEntities;
        [SerializeField] private CanvasInventoryController _inventoryController;
        [SerializeField] private HoveredBlockObserver _observer;
        [SerializeField] private HoveredBlockBreaker _breaker;
        [SerializeField] private HoveredBlockPicker _picker;
        [SerializeField] private HoveredBlockPlacer _placer;
        [SerializeField] private WorldManager _worldManager;

        private void OnEnable()
        {
            _placer.OnBlockSetAttempt += HandleBlockSet;
            _breaker.OnBlockBreakAttempt += HandleBlockBroken;
        }
        private void OnDisable()
        {
            _placer.OnBlockSetAttempt -= HandleBlockSet;
            _breaker.OnBlockBreakAttempt -= HandleBlockBroken;
        }
        private void HandleBlockBroken(WorldPosition wc) =>
            _worldManager.Blocks.BreakVisible(wc);

        private void HandleBlockSet(WorldPosition wc)
        {
            if (!_observer.ReachedLimitPosition && !_picker.SelectedBlock.IsAir)
            {
                ItemInfo itemInfo = _worldManager.ItemDatabase.GetByBlockId(_picker.SelectedBlock.Id);
                if (
                    _inventoryController.Inventory.Has(new ItemStack(itemInfo), _inventoryController.ActiveHotbarIndex) &&
                    !_worldEntities.HasEntityAtPoint(wc) &&
                    _worldManager.Blocks.Set(wc, _picker.SelectedBlock, _picker.SelectedLayer, _picker.SelectedStyles)
                )
                {
                    _inventoryController.Inventory.TryRemove(_inventoryController.ActiveHotbarIndex, 1, out ItemStack removed);
                }
            }
        }
    }
}