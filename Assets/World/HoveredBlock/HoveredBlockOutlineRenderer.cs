using UnityEngine;
using World.Blocks;
using World.Chunks;
using World.Cameras;

namespace World.HoveredBlock
{
    public class HoveredBlockOutlineRenderer : MonoBehaviour
    {
        [SerializeField] private CameraObserver _cameraObserver;
        [SerializeField] private HoveredBlockObserver _blockHoveredObserver;
        [SerializeField] private HoveredBlockBreaker _blockBreakingProcess;
        [SerializeField] private ChunksManager _chunksManager;
        [SerializeField] private SpriteRenderer _targetSpriteRenderer;
        [SerializeField] private Material _blockHoveredMaterial;

        private void Awake()
        {
            OnCameraSizeChanged(_cameraObserver.GetOrthographicSize());
        }
        private void OnEnable()
        {
            _cameraObserver.OnOrthographicSizeChanged += OnCameraSizeChanged;
            _blockHoveredObserver.OnBlockHoveredChanged += OnBlockHoveredChanged;
            _blockBreakingProcess.OnBlockBreakAttempt += OnBlockBreakAttempt;
        }
        private void OnDisable()
        {
            _cameraObserver.OnOrthographicSizeChanged -= OnCameraSizeChanged;
            _blockHoveredObserver.OnBlockHoveredChanged -= OnBlockHoveredChanged;
            _blockBreakingProcess.OnBlockBreakAttempt -= OnBlockBreakAttempt;
        }

        private void SetOutline(WorldPosition worldPosition)
        {
            Block hoveredBlock = _chunksManager.Blocks.GetBreakable(worldPosition, out BlockLayer blockLayer);
            bool enabled = !hoveredBlock.IsAir();

            _targetSpriteRenderer.enabled = enabled;

            if (enabled)
            {
                BlockInfo hoveredInfo = _chunksManager.BlockDatabase.Get(hoveredBlock.Id);
                transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
                _targetSpriteRenderer.color = hoveredInfo.OutlineColor;
            }
        }

        private void OnBlockHoveredChanged(WorldPosition worldPosition)
        {
            SetOutline(worldPosition);
        }
        private void OnBlockBreakAttempt(WorldPosition worldPosition)
        {
            _targetSpriteRenderer.enabled = false;
        }
        private void OnCameraSizeChanged(float newSize)
        {
            _blockHoveredMaterial.SetFloat("_OrthoSize", newSize);
        }
    }
}