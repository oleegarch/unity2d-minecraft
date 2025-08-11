using UnityEngine;
using World.Chunks;

namespace World.HoveredBlock
{
    public class HoveredBlockActionDispatcher : MonoBehaviour
    {
        [SerializeField] private HoveredBlockOutlineRenderer _outliner;
        [SerializeField] private HoveredBlockObserver _observer;
        [SerializeField] private HoveredBlockBreaker _breaker;
        [SerializeField] private HoveredBlockPicker _picker;
        [SerializeField] private HoveredBlockPlacer _placer;
        [SerializeField] private ChunksManager _chunksManager;

        private void OnEnable()
        {
            _placer.OnBlockSetAttempt += HandleBlockSet;
            _breaker.OnBlockBreakAttempt += HandleBlockBroken;
        }
        private void OnDisable()
        {
            _placer.OnBlockSetAttempt -= HandleBlockSet;
            _breaker.OnBlockBreakAttempt -= HandleBlockBroken;
        }
        private void HandleBlockBroken(WorldPosition wc) => _chunksManager.Blocks.BreakVisible(wc);
        private void HandleBlockSet(WorldPosition wc) => _chunksManager.Blocks.Set(wc, _picker.SelectedBlock, _picker.SelectedLayer, _picker.SelectedStyles);
    }
}