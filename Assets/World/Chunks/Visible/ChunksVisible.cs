using UnityEngine;
using System;
using World.Cameras;
using World.Chunks.Generator;

namespace World.Chunks
{
    public interface IChunksVisible
    {
        public event Action<RectInt> OnVisibleChunksChanged;
        public RectInt VisibleRect { get; }
    }
    public class ChunksVisible : MonoBehaviour, IChunksVisible
    {
        [SerializeField] private CameraObserver _cameraObserver;
        [SerializeField] private WorldConfig _chunkGeneratorConfig;

        private int _chunkSize => _chunkGeneratorConfig.GetChunkGeneratorConfig("Earth").ChunkSize;
        private WorldPosition _lastCenterChunk = new WorldPosition(int.MinValue, int.MinValue);

        public event Action<RectInt> OnVisibleChunksChanged;
        
        public RectInt VisibleRect { get; private set; }
        public RectInt BlocksVisibleRect =>
            new RectInt(
                VisibleRect.xMin * _chunkSize,
                VisibleRect.yMin * _chunkSize,
                (VisibleRect.width + 1) * _chunkSize - 1,
                (VisibleRect.height + 1) * _chunkSize - 1
            );

        public void Enable()
        {
            OnCameraPositionChanged(_cameraObserver.GetPosition());
            _cameraObserver.OnPositionChanged += OnCameraPositionChanged;
            _cameraObserver.OnOrthographicSizeChanged += OnCameraSizeChanged;
        }
        public void Disable()
        {
            _cameraObserver.OnPositionChanged -= OnCameraPositionChanged;
            _cameraObserver.OnOrthographicSizeChanged -= OnCameraSizeChanged;
        }

        private void OnCameraPositionChanged(Vector3 position)
        {
            if (TryUpdateCenterChunk(position))
            {
                UpdateRenderRect();
                OnVisibleChunksChanged?.Invoke(VisibleRect);
            }
        }

        private void OnCameraSizeChanged(float newSize)
        {
            UpdateRenderRect();
            OnVisibleChunksChanged?.Invoke(VisibleRect);
        }

        private void UpdateRenderRect()
        {
            float camHeight = _cameraObserver.GetOrthographicSize() * 2f;
            float camWidth = _cameraObserver.GetOrthographicWidth() * 2f;

            int halfChunksCountX = Mathf.CeilToInt(camWidth / _chunkSize / 2f);
            int halfChunksCountY = Mathf.CeilToInt(camHeight / _chunkSize / 2f);

            int xMin = _lastCenterChunk.x - halfChunksCountX;
            int yMin = _lastCenterChunk.y - halfChunksCountY;
            int width = halfChunksCountX * 2;
            int height = halfChunksCountY * 2;

            VisibleRect = new RectInt(xMin, yMin, width, height);
        }

        private bool TryUpdateCenterChunk(Vector3 position)
        {
            WorldPosition center = new WorldPosition(
                Mathf.FloorToInt(position.x / _chunkSize),
                Mathf.FloorToInt(position.y / _chunkSize)
            );

            if (center != _lastCenterChunk)
            {
                _lastCenterChunk = center;
                return true;
            }

            return false;
        }
    }
}