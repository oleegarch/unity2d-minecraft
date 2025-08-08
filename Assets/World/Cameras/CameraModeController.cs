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
            private set => SetGameMode(value);
        }

        private void Awake()
        {
            ApplyInitialGameMode();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                return;

            if (_cameraMode != _currentCameraMode)
                SetGameMode(_cameraMode);
        }
#endif

        private void ApplyInitialGameMode()
        {
            _currentCameraMode = _cameraMode;
            ChangeGameMode();
        }
        private void ChangeGameMode()
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

        public void SetGameMode(CameraMode gameMode)
        {
            if (_currentCameraMode == gameMode)
                return;

            _currentCameraMode = gameMode;
            _cameraMode = gameMode;
            ChangeGameMode();
        }
    }
}