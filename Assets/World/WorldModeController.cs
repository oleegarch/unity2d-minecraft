using UnityEngine;
using System;

namespace World.Cameras
{
    public enum WorldMode
    {
        Survival,
        Creative
    }

    public class WorldModeController : MonoBehaviour
    {
        [SerializeField] private WorldMode _worldMode;

        private WorldMode _currentWorldMode;
        
        public event Action<WorldMode> OnWorldModeChanged;

        public WorldMode WorldMode
        {
            get => _currentWorldMode;
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

            if (_worldMode != _currentWorldMode)
                SetGameMode(_worldMode);
        }
#endif

        private void ApplyInitialGameMode()
        {
            _currentWorldMode = _worldMode;
            ChangeGameMode();
        }
        private void ChangeGameMode()
        {
            switch (_currentWorldMode)
            {
                case WorldMode.Survival:
                    break;
                case WorldMode.Creative:
                    break;
            }

            OnWorldModeChanged?.Invoke(_currentWorldMode);
        }

        public void SetGameMode(WorldMode gameMode)
        {
            if (_currentWorldMode == gameMode)
                return;

            _currentWorldMode = gameMode;
            _worldMode = gameMode;
            ChangeGameMode();
        }
    }
}