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
        [SerializeField] private WorldEnvironmentAccessor _environment;
        [SerializeField] private SpriteRenderer _targetSpriteRenderer;
        [SerializeField] private float _whenReachedLimitColorAlpha = 0.25f;

        private void OnEnable()
        {
            _blockHoveredObserver.OnBlockHoveredChanged += HandleBlockHoveredChanged;
            _blockHoveredObserver.OnLimitedChanged += HandleLimitedChanged;
            _blockBreakingProcess.OnBlockBreakAttempt += HandleBlockBreak;
        }
        private void OnDisable()
        {
            _blockHoveredObserver.OnBlockHoveredChanged -= HandleBlockHoveredChanged;
            _blockHoveredObserver.OnLimitedChanged -= HandleLimitedChanged;
            _blockBreakingProcess.OnBlockBreakAttempt -= HandleBlockBreak;
        }

        private void SetOutline(WorldPosition worldPosition)
        {
            Block hoveredBlock = _worldManager.Blocks.GetBreakable(worldPosition, out BlockLayer blockLayer);
            bool enabled = !hoveredBlock.IsAir;

            _targetSpriteRenderer.enabled = enabled;

            if (enabled)
            {
                transform.position = new Vector3(worldPosition.x, worldPosition.y, 0f);

                // change only rgb, not alpha
                BlockInfo hoveredInfo = _environment.BlockDatabase.Get(hoveredBlock.Id);
                Color rgbChanged = _targetSpriteRenderer.color;
                Color change = hoveredInfo.OutlineColor;
                rgbChanged.r = change.r;
                rgbChanged.g = change.g;
                rgbChanged.b = change.b;
                _targetSpriteRenderer.color = rgbChanged;
            }
        }

        private void HandleBlockHoveredChanged(WorldPosition worldPosition)
        {
            SetOutline(worldPosition);
        }
        private void HandleLimitedChanged(bool limited)
        {
            Color outlineColor = _targetSpriteRenderer.color;
            outlineColor.a = limited ? _whenReachedLimitColorAlpha : 1f;
            _targetSpriteRenderer.color = outlineColor;
        }
        private void HandleBlockBreak(WorldPosition worldPosition)
        {
            _targetSpriteRenderer.enabled = false;
        }
    }
}