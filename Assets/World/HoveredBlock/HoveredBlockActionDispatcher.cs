using UnityEngine;
using World.Chunks;

namespace World.HoveredBlock
{
    public class HoveredBlockActionDispatcher : MonoBehaviour
    {
        [SerializeField] private HoveredBlockOutlineRenderer _outliner;
        [SerializeField] private HoveredBlockObserver _observer;
        [SerializeField] private HoveredBlockBreaker _breaker;
        [SerializeField] private HoveredBlockPicker _selector;
        [SerializeField] private HoveredBlockPlacer _setting;
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