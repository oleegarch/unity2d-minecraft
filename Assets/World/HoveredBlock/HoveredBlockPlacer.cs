using UnityEngine;
using UnityEngine.InputSystem;
using System;
using World.InputActions;
using World.HoveredBlock.BlockStylesSelector;
using World.Blocks;

namespace World.HoveredBlock
{
    public class HoveredBlockPlacer : MonoBehaviour
    {
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private HoveredBlockObserver _blockHoveredObserver;
        [SerializeField] private HoveredBlockPicker _blockHoveredPicker;
        [SerializeField] private UIBlockStyleSelector _blockStylesSelector;

        public event Action<WorldPosition> OnBlockSetAttempt;

        private void OnEnable()
        {
            var actions = _inputManager.Controls.HoveredBlockPlacer;
            actions.MouseFastRightClick.performed += HandleHoveredBlockPlace;
            actions.MouseSlowRightClick.performed += HandleMouseSlowRightClick;
            actions.MouseSlowRightClick.canceled += HandleMouseSlowRightClick;
            actions.Enable();
        }
        private void OnDisable()
        {
            var actions = _inputManager.Controls.HoveredBlockPlacer;
            actions.MouseFastRightClick.performed -= HandleHoveredBlockPlace;
            actions.MouseSlowRightClick.performed -= HandleMouseSlowRightClick;
            actions.MouseSlowRightClick.canceled -= HandleMouseSlowRightClick;
            actions.Disable();
        }
        private void HandleHoveredBlockPlace(InputAction.CallbackContext context)
        {
            OnBlockSetAttempt?.Invoke(_blockHoveredObserver.HoveredPosition);
        }

        private void HandleMouseSlowRightClick(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _blockStylesSelector.StartSelecting(_blockHoveredObserver.HoveredPosition);
            }
            else if (context.canceled)
            {
                BlockPlacementVariant? variantNullable = _blockStylesSelector.Select();
                if (variantNullable.HasValue)
                {
                    _blockHoveredPicker.ChangePlacementVariant(variantNullable.Value);
                    OnBlockSetAttempt?.Invoke(_blockStylesSelector.GetStartedWorldPosition());
                }
            }
        }
    }
}