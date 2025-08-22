using System.Collections.Generic;
using UnityEngine;
using World.Blocks;
using World.Chunks;

namespace World.Systems
{
    public class BreakableByGravitySystem : IWorldSystem
    {
        private ChunksManager _manager;

        private bool HasNonGravitySupport(WorldPosition pos, BlockLayer layer)
        {
            // соседние позиции: влево, вправо, сверху
            var neighbors = new[]
            {
                pos + Vector2Int.left,
                pos + Vector2Int.right,
                pos + Vector2Int.up
            };

            foreach (var npos in neighbors)
            {
                var nb = _manager.Blocks.Get(npos, layer);
                if (nb.IsAir) continue;

                var ninfo = _manager.BlockDatabase.Get(nb.Id);
                if (ninfo == null) continue; // на всякий случай

                if (!ninfo.BreakableByGravity)
                    return true;
            }

            return false;
        }

        // Обработчик события ломания блока
        private void BlockBreakMatcher(WorldPosition position, Block brokenBlock, BlockLayer layer)
        {
            var toBreak = new List<WorldPosition>();
            var visited = new HashSet<WorldPosition>();

            // стартовые кандидаты: блок сверху, слева и справа
            var queue = new Queue<WorldPosition>();
            queue.Enqueue(position + Vector2Int.up);
            queue.Enqueue(position + Vector2Int.left);
            queue.Enqueue(position + Vector2Int.right);

            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();

                if (!visited.Add(pos))
                    continue; // уже проверяли

                var cur = _manager.Blocks.Get(pos, layer);
                if (cur.IsAir)
                    continue;

                var info = _manager.BlockDatabase.Get(cur.Id);
                if (info == null)
                    continue;

                // блок неподвижный — он поддержка, его не ломаем и дальше от него не идём
                if (!info.BreakableByGravity)
                    continue;

                // если блок имеет поддержку — он держится, тоже не ломаем
                if (HasNonGravitySupport(pos, layer))
                    continue;

                // блок падает
                toBreak.Add(pos);

                // кандидаты для проверки — соседи по вертикали и горизонтали
                queue.Enqueue(pos + Vector2Int.up);
                queue.Enqueue(pos + Vector2Int.left);
                queue.Enqueue(pos + Vector2Int.right);
            }

            if (toBreak.Count == 0)
                return;

            // Разрушаем сверху вниз (по Y), чтобы красиво рушилось
            toBreak.Sort((a, b) => b.y.CompareTo(a.y));

            foreach (var bp in toBreak)
                _manager.Blocks.Break(bp, layer);
        }

        public void RegisterSystem(ChunksManager manager)
        {
            _manager = manager;
            _manager.Blocks.Events.OnBlockBroken += BlockBreakMatcher;
        }

        public void Dispose()
        {
            if (_manager != null)
                _manager.Blocks.Events.OnBlockBroken -= BlockBreakMatcher;
            _manager = null;
        }
    }
}