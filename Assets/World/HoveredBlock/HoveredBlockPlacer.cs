using UnityEngine;
using UnityEngine.InputSystem;
using System;
using World.InputActions;

namespace World.HoveredBlock
{
    public class HoveredBlockPlacer : MonoBehaviour
    {
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private HoveredBlockObserver _blockHoveredObserver;

        public event Action<WorldPosition> OnBlockSetAttempt;

        private void OnEnable()
        {
            var actions = _inputManager.Controls.HoveredBlockPlacer;
            actions.MouseRightClick.performed += HandleHoveredBlockPlace;
            actions.Enable();
        }
        private void OnDisable()
        {
            var actions = _inputManager.Controls.HoveredBlockPlacer;
            actions.MouseRightClick.performed -= HandleHoveredBlockPlace;
            actions.Disable();
        }
        private void HandleHoveredBlockPlace(InputAction.CallbackContext context)
        {
            OnBlockSetAttempt?.Invoke(_blockHoveredObserver.HoveredPosition);
        }
    }
}