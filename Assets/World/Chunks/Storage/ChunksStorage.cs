using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using World.Chunks.Blocks;
using World.Utils.Serialization;

namespace World.Chunks.Storage
{
    [Serializable]
    public class ChunksStorageData
    {
        public readonly Dictionary<ChunkIndex, ChunkDiffData> ModifiedChunks;

        public ChunksStorageData()
        {
            ModifiedChunks = new();
        }
    }

    public interface IChunksStorage
    {
        public void CreateDiffForChunk(Chunk chunk);
        public void RemoveDiffFromChunk(Chunk chunk);
        public UniTask Save();
        public UniTask Load();
    }
    public class ChunksStorage : IChunksStorage
    {
        public ChunksStorageData Data;
        public bool Loaded;
        public event Action OnDataLoaded;
        public event Action OnDataChanged;

        private readonly Dictionary<ChunkIndex, ChunkDiff> _currentChunkDiffs;
        private readonly JSONDataFileSystemSaver _saver;
        private readonly string _generatorName;
        private readonly int _seed;
        private string _filename => $"world_{_generatorName}_{_seed}";

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private const string Subfolder = "chunks_dev";
        private const bool PrettyPrint = true;
#else
        private const string Subfolder = "chunks";
        private const bool PrettyPrint = false;
#endif

        public ChunksStorage(string generatorName, int seed)
        {
            _generatorName = generatorName;
            _seed = seed;
            _saver = new JSONDataFileSystemSaver(Subfolder, PrettyPrint);
            _currentChunkDiffs = new();
        }

        /// <summary>
        /// Когда появляется новый чанк в ChunksCreator мы пытаемся его сразу связать с ChunkDiff
        /// Чтобы собирать все изменения произведённые в чанке и в дальнейшем восстанавливать их
        /// </summary>
        /// <param name="chunk">Чанк на основе которого будем создавать или искать ChunkDiff</param>
        /// <returns>Возвращает true если ChunkDiff только что был создан и связан</returns>
        public void CreateDiffForChunk(Chunk chunk)
        {
            if (Loaded == false) throw new InvalidOperationException($"ChunksStorage: it is forbidden to call the TryAddChunkDiff method without loading!");

            ChunkIndex index = chunk.Index;
            if (!_currentChunkDiffs.TryGetValue(index, out ChunkDiff diff))
            {
                if (!Data.ModifiedChunks.TryGetValue(index, out ChunkDiffData data))
                {
                    // СОЗДАНИЕ — ChunkDiffData отсутствует, значит этот Chunk новый и на него не записывались изменения вовсе
                    data = Data.ModifiedChunks[index] = new ChunkDiffData(chunk.Index, chunk.Size);
                    diff = new ChunkDiff(data);
                    diff.LinkChunk(chunk);
                    diff.SubscribeToChunkEvents();
                    _currentChunkDiffs[index] = diff;
                }
                else
                {
                    // ВОССТАНОВЛЕНИЕ — ChunkDiffData есть, но самого ChunkDiff нет, значит изменения записывались или существуют, но Chunk был удалён из словаря
                    diff = new ChunkDiff(data);
                    diff.LinkChunk(chunk);
                    diff.ApplyDiff();
                    diff.SubscribeToChunkEvents();
                    diff.OnDataChanged += HandleDiffDataChanged;
                    _currentChunkDiffs[index] = diff;
                }
            }
        }
        /// <summary>
        /// Когда чанк удаляется нужно так же отписываться от изменений в нём.
        /// Но при этом сам ChunkDiffData не удаляется!
        /// </summary>
        public void RemoveDiffFromChunk(Chunk chunk)
        {
            if (Loaded == false) throw new InvalidOperationException($"ChunksStorage: it is forbidden to call the RemoveDiffFromChunk method without loading!");

            ChunkIndex index = chunk.Index;
            if (_currentChunkDiffs.TryGetValue(index, out ChunkDiff diff))
            {
                diff.UnsubscribeFromChunkEvents();
                diff.UnlinkChunk();
                diff.OnDataChanged -= HandleDiffDataChanged;
                _currentChunkDiffs.Remove(index);
            }
        }

        /// <summary>
        /// Проксируем все изменения в ChunkDiff в локальное событие ChunksStorage.OnDataChanged
        /// </summary>
        public void HandleDiffDataChanged()
        {
            OnDataChanged?.Invoke();
        }

        /// <summary>
        /// Сохранить все изменения через установленный _saver
        /// </summary>
        public async UniTask Save()
        {
            await _saver.SaveAsync(Data, $"{_filename}.json");
        }

        /// <summary>
        /// Загрузить все изменения через установленный _saver
        /// </summary>
        public async UniTask Load()
        {
            Data = await _saver.LoadAsync<ChunksStorageData>($"{_filename}.json");

            if (Data == null)
                Data = new ChunksStorageData();

            Loaded = true;
            OnDataLoaded?.Invoke();
        }
    }
}