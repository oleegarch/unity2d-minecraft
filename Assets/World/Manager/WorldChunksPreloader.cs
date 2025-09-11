using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using World.Chunks.Generator;

namespace World.Chunks
{
    public class WorldChunksPreloader : MonoBehaviour
    {
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private WorldStorage _worldStorage;
        private IChunkGenerator _generator;

        public Dictionary<int, WorldPosition[]> PreloadPositionsByComponent;

        public event Action<HashSet<ChunkIndex>> OnChunksPreload;
        public event Action OnChunksPreloaded;

        private HashSet<ChunkIndex> _allChunkIndexes;
        private bool _isDirty;

        public void Enable()
        {
            _generator = _worldManager.Generator;
            _worldStorage.OnOutChunksUpdated += HandleChunksUpdated;
            PreloadPositionsByComponent = new();
        }
        public void Disable()
        {
            _worldStorage.OnOutChunksUpdated -= HandleChunksUpdated;
            PreloadPositionsByComponent = null;
        }
        private void Update()
        {
            if (_isDirty)
            {
                _allChunkIndexes = new HashSet<ChunkIndex>(PreloadPositionsByComponent.Values.SelectMany(positions => positions.Select(p => p.ToChunkIndex(_generator.ChunkSize))));
                _isDirty = false;
                OnChunksPreload?.Invoke(_allChunkIndexes);
            }
        }
        private void HandleChunksUpdated()
        {
            OnChunksPreloaded?.Invoke();
        }

        public void SetComponentPositions(int id, WorldPosition[] positions)
        {
            if (PreloadPositionsByComponent == null) return;
            PreloadPositionsByComponent[id] = positions;
            _isDirty = true;
        }
        public void DestroyComponent(int id)
        {
            if (PreloadPositionsByComponent == null) return;
            PreloadPositionsByComponent.Remove(id);
            _isDirty = true;
        }
    }
}