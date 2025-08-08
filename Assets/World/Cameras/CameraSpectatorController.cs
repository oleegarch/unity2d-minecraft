using UnityEngine;
using UnityEngine.InputSystem;
using World.InputActions;

namespace World.Cameras
{
    [RequireComponent(typeof(CameraObserver))]
    public class CameraSpectatorController : MonoBehaviour
    {
        [SerializeField] private Camera _targetCamera;
        [SerializeField] private CameraObserver _cameraObserver;
        [SerializeField] private WorldInputManager _inputManager;

        [Header("Mouse Zoom Settings")]
        public float mouseZoomSpeed = 200f;
        public float minZoom = 5f;
        public float maxZoom = 40f;

        [Header("Keyboard Movement Settings")]
        public float keyboardMoveSpeed = 20f;

        private Vector2 _moveKeyboardDirection;
        private bool _isDragging;
        private Vector2 _prevPointerPosition;

        private void Update()
        {
            if(_moveKeyboardDirection != Vector2.zero)
            {
                Vector3 delta = new Vector3(_moveKeyboardDirection.x, _moveKeyboardDirection.y, 0) * keyboardMoveSpeed * Time.deltaTime;
                _cameraObserver.SetPosition(_targetCamera.transform.position + delta);
            }
        }

        private void OnEnable()
        {
            var actions = _inputManager.Controls.CameraSpectator;
            actions.PointerStart.performed += OnPointerStart;
            actions.PointerStart.canceled += OnPointerCanceled;
            actions.PointerMove.performed += OnPointerMove;
            actions.KeyboardMove.performed += OnKeyboardMove;
            actions.KeyboardMove.canceled += OnKeyboardMoveCanceled;
            actions.MouseZoom.performed += OnMouseZoom;
            actions.Enable();

            _cameraObserver.HasSpectatorController = true;
        }

        private void OnDisable()
        {
            var actions = _inputManager.Controls.CameraSpectator;
            actions.PointerStart.performed -= OnPointerStart;
            actions.PointerStart.canceled -= OnPointerCanceled;
            actions.PointerMove.performed -= OnPointerMove;
            actions.KeyboardMove.performed -= OnKeyboardMove;
            actions.KeyboardMove.canceled -= OnKeyboardMoveCanceled;
            actions.MouseZoom.performed -= OnMouseZoom;
            actions.Disable();

            _cameraObserver.HasSpectatorController = false;
        }

        private void OnPointerStart(InputAction.CallbackContext context)
        {
            _isDragging = true;
            _prevPointerPosition = _inputManager.Controls.CameraSpectator.PointerMove.ReadValue<Vector2>();
        }
        private void OnPointerCanceled(InputAction.CallbackContext context)
        {
            _isDragging = false;
        }
        private void OnPointerMove(InputAction.CallbackContext context)
        {
            if (!_isDragging) return;

            // Текущая позиция курсора в экране (px)
            Vector2 pointerPosition = context.ReadValue<Vector2>();

            // Конвертируем обе в мировые
            Vector3 prevWorld = _targetCamera.ScreenToWorldPoint(new Vector3(_prevPointerPosition.x, _prevPointerPosition.y, _targetCamera.nearClipPlane));
            Vector3 currWorld = _targetCamera.ScreenToWorldPoint(new Vector3(pointerPosition.x,  pointerPosition.y,  _targetCamera.nearClipPlane));

            // Дельта в мировых координатах
            Vector3 worldDelta = prevWorld - currWorld;

            // Сдвигаем камеру
            _cameraObserver.SetPosition(_targetCamera.transform.position - worldDelta);

            // Запоминаем текущую для следующего кадра
            _prevPointerPosition = pointerPosition;
        }

        private void OnKeyboardMove(InputAction.CallbackContext context)
        {
            _moveKeyboardDirection = context.ReadValue<Vector2>();
        }
        private void OnKeyboardMoveCanceled(InputAction.CallbackContext context)
        {
            _moveKeyboardDirection = Vector2.zero;
        }

        private void OnMouseZoom(InputAction.CallbackContext context)
        {
            float scrollDelta = context.ReadValue<float>();
            var newSize = _targetCamera.orthographicSize - scrollDelta * mouseZoomSpeed * Time.deltaTime;
            _cameraObserver.SetOrthographicSize(Mathf.Clamp(newSize, minZoom, maxZoom));
        }
    }
}