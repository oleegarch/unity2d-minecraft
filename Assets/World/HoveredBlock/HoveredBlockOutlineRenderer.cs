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
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private SpriteRenderer _targetSpriteRenderer;
        [SerializeField] private Material _blockHoveredMaterial;

        private void Awake()
        {
            HandleCameraSizeChanged(_cameraObserver.GetOrthographicSize());
        }
        private void OnEnable()
        {
            _cameraObserver.OnOrthographicSizeChanged += HandleCameraSizeChanged;
            _blockHoveredObserver.OnBlockHoveredChanged += HandleBlockHoveredChanged;
            _blockBreakingProcess.OnBlockBreakAttempt += HandleBlockBreak;
        }
        private void OnDisable()
        {
            _cameraObserver.OnOrthographicSizeChanged -= HandleCameraSizeChanged;
            _blockHoveredObserver.OnBlockHoveredChanged -= HandleBlockHoveredChanged;
            _blockBreakingProcess.OnBlockBreakAttempt -= HandleBlockBreak;
        }

        private void SetOutline(WorldPosition worldPosition)
        {
            Block hoveredBlock = _worldManager.Blocks.GetBreakable(worldPosition, out BlockLayer blockLayer);
            bool enabled = !hoveredBlock.IsAir;

            _targetSpriteRenderer.enabled = enabled;

            if (enabled)
            {
                BlockInfo hoveredInfo = _worldManager.BlockDatabase.Get(hoveredBlock.Id);
                transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);
                _targetSpriteRenderer.color = hoveredInfo.OutlineColor;
            }
        }

        private void HandleBlockHoveredChanged(WorldPosition worldPosition)
        {
            SetOutline(worldPosition);
        }
        private void HandleBlockBreak(WorldPosition worldPosition)
        {
            _targetSpriteRenderer.enabled = false;
        }
        private void HandleCameraSizeChanged(float newSize)
        {
            _blockHoveredMaterial.SetFloat("_OrthoSize", newSize);
        }
    }
}