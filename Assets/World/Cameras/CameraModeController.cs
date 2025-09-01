using UnityEngine;
using System;
using World.Entities.Player;

namespace World.Cameras
{
    public enum CameraMode
    {
        Spectator,
        Player
    }

    public class CameraModeController : MonoBehaviour
    {
        [SerializeField] private CameraFollowToRigidbody2D _cameraFollow;
        [SerializeField] private CameraSpectatorController _cameraSpectatorController;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private CameraMode _cameraMode;

        private CameraMode _currentCameraMode;
        
        public event Action<CameraMode> OnCameraModeChanged;

        public CameraMode CameraMode
        {
            get => _currentCameraMode;
            private set => SetCameraMode(value);
        }

        private void Awake()
        {
            ApplyInitialCameraMode();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                return;

            if (_cameraMode != _currentCameraMode)
                SetCameraMode(_cameraMode);
        }
#endif

        private void ApplyInitialCameraMode()
        {
            _currentCameraMode = _cameraMode;
            ChangeCameraMode();
        }
        private void ChangeCameraMode()
        {
            switch (_currentCameraMode)
            {
                case CameraMode.Player:
                    _cameraSpectatorController.enabled = false;
                    _cameraFollow.enabled = true;
                    _playerController.enabled = true;
                    break;
                case CameraMode.Spectator:
                    _cameraSpectatorController.enabled = true;
                    _cameraFollow.enabled = false;
                    _playerController.enabled = false;
                    break;
            }

            OnCameraModeChanged?.Invoke(_currentCameraMode);
        }

        public void SetCameraMode(CameraMode gameMode)
        {
            if (_currentCameraMode == gameMode)
                return;

            _currentCameraMode = gameMode;
            _cameraMode = gameMode;
            ChangeCameraMode();
        }
    }
}