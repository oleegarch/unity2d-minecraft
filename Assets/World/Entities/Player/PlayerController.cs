using UnityEngine;
using UnityEngine.InputSystem;
using World.InputActions;

namespace World.Entities.Player
{
    [RequireComponent(typeof(EntityMovement))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private EntityMovement _movement;

        private void OnEnable()
        {
            var actions = _inputManager.Controls.Player;
            actions.Move.performed += OnMove;
            actions.Move.canceled  += OnMove;
            actions.Jump.performed += OnJump;
            actions.Jump.canceled  += OnJump;
            actions.Enable();
        }

        private void OnDisable()
        {
            var actions = _inputManager.Controls.Player;
            actions.Move.performed -= OnMove;
            actions.Move.canceled  -= OnMove;
            actions.Jump.performed -= OnJump;
            actions.Jump.canceled  -= OnJump;
            actions.Disable();
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _movement.Move(ctx.canceled ? 0f : ctx.ReadValue<float>());
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            _movement.Jump(!ctx.canceled);
        }
    }
}