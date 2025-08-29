using UnityEngine;
using UnityEngine.InputSystem;
using System;
using World.Cameras;
using World.InputActions;

namespace World.HoveredBlock
{
    public class HoveredBlockObserver : MonoBehaviour
    {
        [SerializeField] private HoveredBlockDistanceLineRenderer _lineRenderer;
        [SerializeField] private CameraObserver _cameraObserver;
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private Transform _chunksParent;

        private Vector2 _pointerPosition;

        private Vector2 _cursorPosition = new Vector2(float.MinValue, float.MinValue);
        private Vector2 _cursorPositionInChunks = new Vector2(float.MinValue, float.MinValue);
        private WorldPosition _hovered = new WorldPosition(int.MinValue, int.MinValue);
        private bool _reachedLimitPosition = false;

        public Vector2 CursorPosition => _cursorPosition;
        public Vector2 CursorPositionInChunks => _cursorPositionInChunks;
        public WorldPosition HoveredPosition => _hovered;
        public bool ReachedLimitPosition => _reachedLimitPosition;

        public event Action<WorldPosition> OnBlockHoveredChanged;
        public event Action<bool> OnLimitedChanged;

        private Vector2 _offset;

        private void Awake()
        {
            _offset = new Vector2(_chunksParent.position.x, _chunksParent.position.y);
        }

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
            _cursorPosition = _cameraObserver.Camera.ScreenToWorldPoint(screenPosition);
            _cursorPositionInChunks = _cursorPosition - _offset;

            WorldPosition worldPosition = new WorldPosition(Mathf.FloorToInt(_cursorPositionInChunks.x), Mathf.FloorToInt(_cursorPositionInChunks.y));
            if (worldPosition == _hovered)
                return;

            _hovered = worldPosition;
            OnBlockHoveredChanged?.Invoke(_hovered);
        }
        private void SetLimitedHovered()
        {
            bool limited = Vector2.Distance(_lineRenderer.Start, _hovered.ToVector2Int()) > _lineRenderer.MaxDistance;

            if (limited == _reachedLimitPosition)
                return;

            _reachedLimitPosition = limited;
            OnLimitedChanged?.Invoke(_reachedLimitPosition);
        }

        private void HandleCameraPositionChanged(Vector3 cameraPosition)
        {
            SetHovered(_pointerPosition);
            SetLimitedHovered();
        }
        private void HandlePointerMove(InputAction.CallbackContext context)
        {
            _pointerPosition = context.ReadValue<Vector2>();
            SetHovered(_pointerPosition);
            SetLimitedHovered();
        }
    }
}