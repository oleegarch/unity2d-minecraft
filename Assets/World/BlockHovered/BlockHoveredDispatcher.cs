using UnityEngine;
using World.Chunks;

namespace World.BlockHovered
{
    public class BlockHoveredDispatcher : MonoBehaviour
    {
        [SerializeField] private BlockHoveredOutline _outliner;
        [SerializeField] private BlockHoveredObserver _observer;
        [SerializeField] private BlockBreakingProcess _breaker;
        [SerializeField] private BlockSelector _selector;
        [SerializeField] private BlockSetting _setting;
        [SerializeField] private ChunksManager _chunksManager;

        private void OnEnable()
        {
            _setting.OnBlockSetAttempt += HandleBlockSet;
            _breaker.OnBlockBreakAttempt += HandleBlockBroken;
        }
        private void OnDisable()
        {
            _setting.OnBlockSetAttempt -= HandleBlockSet;
            _breaker.OnBlockBreakAttempt -= HandleBlockBroken;
        }
        private void HandleBlockBroken(WorldPosition wc) => _chunksManager.Blocks.BreakVisible(wc);
        private void HandleBlockSet(WorldPosition wc) => _chunksManager.Blocks.Set(wc, _selector.SelectedBlock, _selector.SelectedLayer);
    }
}