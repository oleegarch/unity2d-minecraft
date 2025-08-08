using UnityEngine;
using UnityEngine.InputSystem;
using World.InputActions;

namespace World.Cameras
{
    [RequireComponent(typeof(CameraObserver))]
    public class CameraZoomController : MonoBehaviour
    {
        [SerializeField] private Camera _targetCamera;
        [SerializeField] private CameraObserver _cameraObserver;
        [SerializeField] private WorldInputManager _inputManager;

        [Header("Mouse Zoom Settings")]
        public float mouseZoomSpeed = 200f;
        public float minZoom = 5f;
        public float maxZoom = 40f;

        private void OnEnable()
        {
            var actions = _inputManager.Controls.CameraZoom;
            actions.MouseZoom.performed += OnMouseZoom;
            actions.Enable();
        }

        private void OnDisable()
        {
            var actions = _inputManager.Controls.CameraZoom;
            actions.MouseZoom.performed -= OnMouseZoom;
            actions.Disable();
        }

        private void OnMouseZoom(InputAction.CallbackContext context)
        {
            float scrollDelta = context.ReadValue<float>();
            var newSize = _targetCamera.orthographicSize - scrollDelta * mouseZoomSpeed * Time.deltaTime;
            _cameraObserver.SetOrthographicSize(Mathf.Clamp(newSize, minZoom, maxZoom));
        }
    }
}