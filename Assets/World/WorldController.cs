using UnityEngine;
using System;
using World.Cameras;
using World.Entities.Player;

namespace World
{
    public enum GameMode
    {
        Spectator,
        Player
    }

    public class WorldController : MonoBehaviour
    {
        [SerializeField] private CameraFollowToRigidbody2D _cameraFollow;
        [SerializeField] private CameraSpectatorController _cameraSpectatorController;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private GameMode _gameMode;

        private GameMode _currentGameMode;

        public event Action OnWorldInited;
        public event Action OnWorldDestroyed;
        public event Action<GameMode> OnGameModeChanged;

        public GameMode GameMode
        {
            get => _currentGameMode;
            private set => SetGameMode(value);
        }

        private void Awake()
        {
            ApplyInitialGameMode();
            OnWorldInited?.Invoke();
        }

        private void OnDestroy()
        {
            OnWorldDestroyed?.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                return;

            if (_gameMode != _currentGameMode)
                SetGameMode(_gameMode);
        }
#endif

        private void ApplyInitialGameMode()
        {
            _currentGameMode = _gameMode;
            ChangeGameMode();
        }
        private void ChangeGameMode()
        {
            switch (_currentGameMode)
            {
                case GameMode.Player:
                    _cameraSpectatorController.enabled = false;
                    _cameraFollow.enabled = true;
                    _playerController.enabled = true;
                    break;
                case GameMode.Spectator:
                    _cameraSpectatorController.enabled = true;
                    _cameraFollow.enabled = false;
                    _playerController.enabled = false;
                    break;
            }

            OnGameModeChanged?.Invoke(_currentGameMode);
        }

        public void SetGameMode(GameMode gameMode)
        {
            if (_currentGameMode == gameMode)
                return;

            _currentGameMode = gameMode;
            _gameMode = gameMode;
            ChangeGameMode();
        }
    }
}