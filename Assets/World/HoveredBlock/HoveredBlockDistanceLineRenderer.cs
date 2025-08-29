using System;
using UnityEngine;

namespace World.HoveredBlock
{
    public class HoveredBlockDistanceLineRenderer : MonoBehaviour
    {
        [SerializeField] private Transform _handItemTransform;
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private HoveredBlockObserver _blockHoveredObserver;
        [SerializeField] private float _maxDistance = 5f;

        public float MaxDistance => _maxDistance;
        public Vector2 Start => _handItemTransform.position;

        private void OnEnable()
        {
            _blockHoveredObserver.OnLimitedChanged += HandleLimitedChanged;
        }
        private void OnDisable()
        {
            _blockHoveredObserver.OnLimitedChanged -= HandleLimitedChanged;
        }
        private void Update()
        {
            if (_lineRenderer.enabled)
            {
                Vector2 start = _handItemTransform.position;
                Vector2 end = _blockHoveredObserver.CursorPosition;
                Vector2 rawOffset = end - start;
                float rawDist = rawOffset.magnitude;
                if (rawDist > _maxDistance)
                    end = start + rawOffset.normalized * _maxDistance;

                // Рисуем линию
                _lineRenderer.SetPosition(0, start);
                _lineRenderer.SetPosition(1, end);
            }
        }

        private void HandleLimitedChanged(bool limited)
        {
            _lineRenderer.enabled = limited;
        }
    }
}