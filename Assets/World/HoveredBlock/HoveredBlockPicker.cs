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

        public Block SelectedBlock => _selectedBlock;
        public BlockLayer SelectedLayer => _selectedLayer;
        public event Action<WorldPosition, Block, BlockLayer> OnBlockSelectedChanged;

        private void OnEnable()
        {
            var actions = _inputManager.Controls.HoveredBlockPicker;
            actions.MouseMiddleClick.performed += OnBlockSelect;
            actions.Enable();
        }
        private void OnDisable()
        {
            var actions = _inputManager.Controls.HoveredBlockPicker;
            actions.MouseMiddleClick.performed -= OnBlockSelect;
            actions.Disable();
        }
        private void OnBlockSelect(InputAction.CallbackContext context)
        {
            WorldPosition worldPosition = _blockHoveredObserver.HoveredPosition;
            _selectedBlock = _chunksManager.Blocks.GetBreakable(worldPosition, out _selectedLayer);

            OnBlockSelectedChanged?.Invoke(worldPosition, _selectedBlock, _selectedLayer);
        }
    }
}