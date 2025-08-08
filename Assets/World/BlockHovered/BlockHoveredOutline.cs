using UnityEngine;
using World.Blocks;
using World.Chunks;
using World.Cameras;

namespace World.BlockHovered
{
    public class BlockHoveredOutline : MonoBehaviour
    {
        [SerializeField] private CameraObserver _cameraObserver;
        [SerializeField] private BlockHoveredObserver _blockHoveredObserver;
        [SerializeField] private BlockBreakingProcess _blockBreakingProcess;
        [SerializeField] private ChunksManager _chunksManager;
        [SerializeField] private BlockDatabase _blockDatabase;
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
            _blockBreakingProcess.OnBlockBroken += OnBlockBroken;
        }
        private void OnDisable()
        {
            _cameraObserver.OnOrthographicSizeChanged -= OnCameraSizeChanged;
            _blockHoveredObserver.OnBlockHoveredChanged -= OnBlockHoveredChanged;
            _blockBreakingProcess.OnBlockBroken -= OnBlockBroken;
        }

        private void SetOutline(WorldPosition worldPosition)
        {
            Block hoveredBlock = _chunksManager.Blocks.GetBreakable(worldPosition, out BlockLayer blockLayer);
            bool enabled = !hoveredBlock.IsAir();

            _targetSpriteRenderer.enabled = enabled;

            if (enabled)
            {
                BlockInfo hoveredInfo = _blockDatabase.Get(hoveredBlock.Id);
                transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
                _targetSpriteRenderer.color = hoveredInfo.OutlineColor;
            }
        }

        private void OnBlockHoveredChanged(WorldPosition worldPosition)
        {
            SetOutline(worldPosition);
        }
        private void OnBlockBroken(WorldPosition worldPosition)
        {
            _targetSpriteRenderer.enabled = false;
        }
        private void OnCameraSizeChanged(float newSize)
        {
            _blockHoveredMaterial.SetFloat("_OrthoSize", newSize);
        }
    }
}