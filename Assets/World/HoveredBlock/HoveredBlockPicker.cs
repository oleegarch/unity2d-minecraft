using UnityEngine;
using UnityEngine.InputSystem;
using System;
using World.InputActions;
using World.Chunks;
using World.Blocks;
using World.Inventories;

namespace World.HoveredBlock
{
    public class HoveredBlockPicker : MonoBehaviour
    {
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private HoveredBlockObserver _blockHoveredObserver;
        [SerializeField] private PlayerInventoryController _inventoryController;
        [SerializeField] private WorldManager _worldManager;

        public event Action<WorldPosition, Block, BlockLayer, BlockStyles> OnBlockPickedChanged;

        private BlockLayer _selectedLayer;
        private BlockStyles _selectedStyles;
        public BlockLayer SelectedLayer => _selectedLayer;
        public BlockStyles SelectedStyles => _selectedStyles;
        public Block SelectedBlock
        {
            get
            {
                if (
                    _inventoryController.ActiveItemInfo == null ||
                    _inventoryController.ActiveItemInfo.BlockId == Block.AirId
                )
                {
                    return Block.Air;
                }

                return new Block(_inventoryController.ActiveItemInfo.BlockId);
            }
        }

        private void OnEnable()
        {
            var actions = _inputManager.Controls.HoveredBlockPicker;
            actions.MouseMiddleClick.performed += HandleMouseMiddleClick;
            actions.Enable();
        }
        private void OnDisable()
        {
            var actions = _inputManager.Controls.HoveredBlockPicker;
            actions.MouseMiddleClick.performed -= HandleMouseMiddleClick;
            actions.Disable();
        }
        private void HandleMouseMiddleClick(InputAction.CallbackContext context)
        {
            WorldPosition worldPosition = _blockHoveredObserver.HoveredPosition;
            Block selectedBlock = _worldManager.Blocks.GetBreakable(worldPosition, out _selectedLayer);
            _selectedStyles = _worldManager.Blocks.GetBlockStyles(worldPosition, _selectedLayer);

            OnBlockPickedChanged?.Invoke(worldPosition, selectedBlock, _selectedLayer, _selectedStyles);
        }

        public void ChangePlacementVariant(BlockPlacementVariant variant)
        {
            _selectedLayer = variant.Layer;
            _selectedStyles = variant.StylesOverrides;
        }
    }
}