using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using World.Chunks.Blocks;

namespace World.Chunks.Storage
{
    [Serializable]
    public class ChunksStorage
    {
        public Dictionary<ChunkIndex, ChunkDiff> ModifiedChunks;

        // название файла по умолчанию
        private const string DefaultFileName = "modified_chunks.json";

        public ChunksStorage()
        {
            ModifiedChunks = new();
        }
        public ChunksStorage(Dictionary<ChunkIndex, ChunkDiff> alreadyModifiedChunks)
        {
            ModifiedChunks = alreadyModifiedChunks;
        }

        /// <summary>
        /// Когда появляется новый чанк в ChunksCreator мы пытаемся его сразу связать с ChunkDiff
        /// Чтобы собирать все изменения произведённые в чанке и в дальнейшем восстанавливать их
        /// </summary>
        /// <param name="chunk">Чанк на основе которого будем создавать ChunkDiff</param>
        /// <param name="diff">В результате так же будет доступен сам ChunkDiff</param>
        /// <returns>Возвращает true если ChunkDiff только что был создан и связан</returns>
        public bool TryAddChunkDiff(Chunk chunk, out ChunkDiff diff)
        {
            ChunkIndex index = chunk.Index;
            if (!ModifiedChunks.TryGetValue(index, out diff))
            {
                diff = new ChunkDiff(chunk);
                diff.SubscribeToChunkEvents();
                ModifiedChunks[index] = diff;
                return true;
            }

            if (!diff.IsApplied)
            {
                diff.ApplyChunk(chunk);
            }
            if (!diff.IsSubscribedToEvents)
            {
                diff.SubscribeToChunkEvents();
            }

            return false;
        }

        public async UniTask Save(string fileName = DefaultFileName)
        {
            
        }

        public async UniTask Load(string fileName = DefaultFileName)
        {
            
        }
    }
}