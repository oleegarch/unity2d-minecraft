using UnityEngine;
using System;

namespace World
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
            private set => SetWorldMode(value);
        }

        private void Awake()
        {
            ApplyInitialWorldMode();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                return;

            if (_worldMode != _currentWorldMode)
                SetWorldMode(_worldMode);
        }
#endif

        private void ApplyInitialWorldMode()
        {
            _currentWorldMode = _worldMode;
            ChangeWorldMode();
        }
        private void ChangeWorldMode()
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

        public void SetWorldMode(WorldMode gameMode)
        {
            if (_currentWorldMode == gameMode)
                return;

            _currentWorldMode = gameMode;
            _worldMode = gameMode;
            ChangeWorldMode();
        }
    }
}