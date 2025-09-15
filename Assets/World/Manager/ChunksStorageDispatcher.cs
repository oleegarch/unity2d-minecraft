using System;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace World.Chunks.Storage
{
    public class ChunksStorageDispatcher : MonoBehaviour, IDisposable
    {
        [SerializeField] private WorldEnvironmentAccessor _environment;
        [SerializeField] private ChunksCreator _creator;

        private CompositeDisposable _chunksDisposables;
        private IDisposable _subToWorldGenerator;
        public BehaviorSubject<IChunksStorage> OnChunksStorageLoaded = new(null);

        private void Awake()
        {
            _chunksDisposables = new();
            _subToWorldGenerator = _environment.OnWorldGeneratorChanged.Subscribe(name => _ = HandleChunksStorageChanged());
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public async UniTask HandleChunksStorageChanged()
        {
            await _environment.CurrentChunksStorage.Load();

            _chunksDisposables.Dispose();
            _chunksDisposables.Add(_creator.OnChunkCreated.Subscribe(_environment.CurrentChunksStorage.CreateDiffForChunk));
            _chunksDisposables.Add(_creator.OnChunkBeforeRemove.Subscribe(_environment.CurrentChunksStorage.RemoveDiffFromChunk));

            OnChunksStorageLoaded.OnNext(_environment.CurrentChunksStorage);
        }

        public void Dispose()
        {
            _subToWorldGenerator?.Dispose();
            _chunksDisposables.Dispose();
        }
    }
}