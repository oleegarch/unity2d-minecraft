using UnityEngine;
using UnityEngine.InputSystem;
using System;
using World.InputActions;

namespace World.BlockHovered
{
    public class BlockSetting : MonoBehaviour
    {
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private BlockHoveredObserver _blockHoveredObserver;

        public event Action<WorldPosition> OnBlockSetAttempt;

        private void OnEnable()
        {
            var actions = _inputManager.Controls.BlockSetting;
            actions.MouseRightClick.performed += OnBlockSetting;
            actions.Enable();
        }
        private void OnDisable()
        {
            var actions = _inputManager.Controls.BlockSetting;
            actions.MouseRightClick.performed -= OnBlockSetting;
            actions.Disable();
        }
        private void OnBlockSetting(InputAction.CallbackContext context)
        {
            OnBlockSetAttempt?.Invoke(_blockHoveredObserver.HoveredPosition);
        }
    }
}