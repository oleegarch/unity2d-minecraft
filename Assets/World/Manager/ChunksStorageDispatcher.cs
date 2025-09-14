using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace World.Chunks.Storage
{
    public class ChunksStorageDispatcher : MonoBehaviour, IDisposable
    {
        [SerializeField] private WorldEnvironmentAccessor _environment;
        [SerializeField] private ChunksCreator _creator;

        public event Action OnChunksStorageLoaded;

        private void OnDestroy()
        {
            Dispose();
        }

        public void Initialize()
        {
            _creator.OnChunkCreated += _environment.CurrentChunksStorage.CreateDiffForChunk;
            _creator.OnChunkBeforeRemove += _environment.CurrentChunksStorage.RemoveDiffFromChunk;
        }

        public async UniTask Load()
        {
            await _environment.CurrentChunksStorage.Load();
            OnChunksStorageLoaded?.Invoke();
        }

        public void Dispose()
        {
            _creator.OnChunkCreated -= _environment.CurrentChunksStorage.CreateDiffForChunk;
            _creator.OnChunkBeforeRemove -= _environment.CurrentChunksStorage.RemoveDiffFromChunk;
        }
    }
}