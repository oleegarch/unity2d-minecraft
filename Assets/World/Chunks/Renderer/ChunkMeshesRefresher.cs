using System.Collections.Generic;
using World.Blocks;

namespace World.Chunks
{
    public class ChunkMeshesRefresher
    {
        private readonly HashSet<ChunkMeshData> _pending = new HashSet<ChunkMeshData>();

        // Пометить на refresh (без дублей)
        public void ScheduleRefresh(ChunkMeshData meshData)
        {
            if (meshData == null) return;
            _pending.Add(meshData);
        }
        public void ScheduleRefresh(IEnumerable<ChunkMeshData> meshDatas)
        {
            if (meshDatas == null) return;
            _pending.UnionWith(meshDatas);
        }

        // Убрать всё из очереди
        public void UnscheduleAll()
        {
            _pending.Clear();
        }

        // Выполняется в конце фазы update — объединяет все запросы и делает один Refresh на каждый meshData
        public void Refresh(BlockDatabase blockDatabase)
        {
            if (_pending.Count == 0) return;

            // Скопируем в массив, чтобы можно было безопасно добавлять в _pending из обработчиков
            var toProcess = new ChunkMeshData[_pending.Count];
            _pending.CopyTo(toProcess);
            _pending.Clear();

            for (int i = 0; i < toProcess.Length; i++)
            {
                var md = toProcess[i];
                if (md == null) continue;
                md.RefreshMesh(blockDatabase);
            }
        }
    }
}