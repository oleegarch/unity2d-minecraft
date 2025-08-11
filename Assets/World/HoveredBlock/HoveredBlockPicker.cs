using UnityEngine;
using UnityEngine.InputSystem;
using System;
using World.InputActions;
using World.Chunks;
using World.Blocks;

namespace World.HoveredBlock
{
    public class HoveredBlockPicker : MonoBehaviour
    {
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private HoveredBlockObserver _blockHoveredObserver;
        [SerializeField] private ChunksManager _chunksManager;

        private Block _selectedBlock;
        private BlockLayer _selectedLayer;
        private BlockStyles _selectedStyles;

        public Block SelectedBlock => _selectedBlock;
        public BlockLayer SelectedLayer => _selectedLayer;
        public BlockStyles SelectedStyles => _selectedStyles;
        public event Action<WorldPosition, Block, BlockLayer, BlockStyles> OnBlockSelectedChanged;

        private void OnEnable()
        {
            var actions = _inputManager.Controls.HoveredBlockPicker;
            actions.MouseMiddleClick.performed += HandleBlockPick;
            actions.Enable();
        }
        private void OnDisable()
        {
            var actions = _inputManager.Controls.HoveredBlockPicker;
            actions.MouseMiddleClick.performed -= HandleBlockPick;
            actions.Disable();
        }
        private void HandleBlockPick(InputAction.CallbackContext context)
        {
            WorldPosition worldPosition = _blockHoveredObserver.HoveredPosition;
            _selectedBlock = _chunksManager.Blocks.GetBreakable(worldPosition, out _selectedLayer);
            _selectedStyles = _chunksManager.Blocks.GetBlockStyles(worldPosition, _selectedLayer);

            OnBlockSelectedChanged?.Invoke(worldPosition, _selectedBlock, _selectedLayer, _selectedStyles);
        }
    }
}