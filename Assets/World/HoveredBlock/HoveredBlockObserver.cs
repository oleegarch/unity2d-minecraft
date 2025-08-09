using UnityEngine;
using UnityEngine.InputSystem;
using System;
using World.Chunks;
using World.Cameras;
using World.InputActions;

namespace World.HoveredBlock
{
    public class HoveredBlockObserver : MonoBehaviour
    {
        [SerializeField] private CameraObserver _cameraObserver;
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private ChunksManager _chunksManager;
        [SerializeField] private Transform _chunksParent;

        private Vector2 _pointerPosition;
        private WorldPosition _hovered = new WorldPosition(int.MinValue, int.MinValue);

        public WorldPosition HoveredPosition => _hovered;
        public event Action<WorldPosition> OnBlockHoveredChanged;

        private void OnEnable()
        {
            var actions = _inputManager.Controls.BlockHovered;
            actions.PointerMove.performed += HandlePointerMove;
            actions.Enable();

            _cameraObserver.OnPositionChanged += HandleCameraPositionChanged;
        }
        private void OnDisable()
        {
            var actions = _inputManager.Controls.BlockHovered;
            actions.PointerMove.performed -= HandlePointerMove;
            actions.Disable();

            _cameraObserver.OnPositionChanged -= HandleCameraPositionChanged;
        }

        private void SetHovered(Vector2 screenPosition)
        {
            Vector3 chunksPosition = _cameraObserver.Camera.ScreenToWorldPoint(screenPosition) - _chunksParent.position;
            WorldPosition worldPosition = new WorldPosition(Mathf.FloorToInt(chunksPosition.x), Mathf.FloorToInt(chunksPosition.y));

            if (worldPosition == _hovered)
                return;

            _hovered = worldPosition;

            OnBlockHoveredChanged?.Invoke(_hovered);
        }

        private void HandleCameraPositionChanged(Vector3 position)
        {
            SetHovered(_pointerPosition);
        }
        private void HandlePointerMove(InputAction.CallbackContext context)
        {
            _pointerPosition = context.ReadValue<Vector2>();
            SetHovered(_pointerPosition);
        }
    }
}