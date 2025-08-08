using UnityEngine;
using System;

namespace World.Cameras
{
    public class CameraObserver : MonoBehaviour
    {
        [SerializeField] private Camera _targetCamera;

        [NonSerialized] public bool HasSpectatorController = false;

        public event Action<Vector3> OnPositionChanged;
        public event Action<float> OnOrthographicSizeChanged;

        public void SetPosition(Vector3 position)
        {
            if (_targetCamera.transform.position != position)
            {
                _targetCamera.transform.position = position;
                OnPositionChanged?.Invoke(position);
            }
        }

        public void SetOrthographicSize(float size)
        {
            if (!Mathf.Approximately(_targetCamera.orthographicSize, size))
            {
                _targetCamera.orthographicSize = size;
                OnOrthographicSizeChanged?.Invoke(size);
            }
        }

        public void SetBackgroundColor(Color color)
        {
            _targetCamera.backgroundColor = color;
        }

        public Vector3 GetPosition() => _targetCamera.transform.position;
        public float GetSize() => _targetCamera.orthographicSize;
        public float GetOrthographicWidth() => _targetCamera.orthographicSize * _targetCamera.aspect;
    }
}