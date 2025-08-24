using System.Collections.Generic;
using UnityEngine;
using World.Blocks;
using World.Chunks;

namespace World.Systems
{
    public class BreakableByGravitySystem : IWorldSystem
    {
        private WorldManager _manager;

        // есть ли у узла "горизонтальная" цепочка до узла с опорой снизу?
        // цепочка идёт ТОЛЬКО по лево/право, узлы для цепочки — любые не-воздух.
        // опора снизу = под узлом стоит любой не-воздух, который не помечен к ломанию.
        private bool HasSupportPathToGround(WorldPosition start, BlockLayer layer, HashSet<WorldPosition> planned)
        {
            var q = new Queue<WorldPosition>();
            var seen = new HashSet<WorldPosition>();
            q.Enqueue(start);

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                if (!seen.Add(p)) continue;

                // если этот узел сам помечен к ломанию — он не держит никого
                if (planned.Contains(p)) continue;

                var b = _manager.Blocks.Get(p, layer);
                if (b.IsAir) continue;

                // проверяем НИЗ
                var below = p + Vector2Int.down;
                if (!planned.Contains(below))
                {
                    var bb = _manager.Blocks.Get(below, layer);
                    if (!bb.IsAir)
                        return true; // нашли опору снизу
                }

                // движемся только по горизонтали
                var left = p + Vector2Int.left;
                var right = p + Vector2Int.right;

                // добавляем соседей, если там не воздух и они не "виртуально" сломаны
                var lb = _manager.Blocks.Get(left, layer);
                if (!lb.IsAir && !planned.Contains(left)) q.Enqueue(left);

                var rb = _manager.Blocks.Get(right, layer);
                if (!rb.IsAir && !planned.Contains(right)) q.Enqueue(right);
            }

            return false;
        }

        // главный обработчик: «вверх и по бокам», c переоценкой при расширении planned
        private void BlockBreakMatcher(WorldPosition start, Block brokenBlock, BlockLayer layer)
        {
            var toBreak = new List<WorldPosition>();
            var planned = new HashSet<WorldPosition> { start }; // начально считаем нижний уже сломанным

            // processedVersion: чтобы переоценивать узлы, когда план ломания растёт
            int version = 0;
            var processedAtVersion = new Dictionary<WorldPosition, int>();

            var queue = new Queue<WorldPosition>();
            queue.Enqueue(start + Vector2Int.up); // стартуем только вверх

            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();

                // если уже обрабатывали этот узел при текущем объёме planned — можно пропустить
                if (processedAtVersion.TryGetValue(pos, out var v) && v == version)
                    continue;
                processedAtVersion[pos] = version;

                var cur = _manager.Blocks.Get(pos, layer);
                if (cur.IsAir) continue;

                var info = _manager.BlockDatabase.Get(cur.Id);
                if (info == null) continue;

                // ломаем только гравитационные блоки; негравитационные участвуют лишь как часть цепочки поддержки
                if (!info.BreakableByGravity) continue;

                // проверяем, имеет ли этот узел КАКУЮ-ЛИБО цепочку поддержки до низа (через лево/право)
                if (HasSupportPathToGround(pos, layer, planned))
                {
                    // поддержка есть — этот узел не рушим, вверх от него тоже не пойдём
                    continue;
                }

                // поддержки нет — планируем ломание
                if (planned.Add(pos))
                {
                    toBreak.Add(pos);

                    // план изменился -> увеличиваем версию, чтобы переоценить ранее просмотренных соседей
                    version++;

                    // продолжаем цепочку: вверх и по бокам
                    queue.Enqueue(pos + Vector2Int.up);
                    queue.Enqueue(pos + Vector2Int.left);
                    queue.Enqueue(pos + Vector2Int.right);
                }
            }

            if (toBreak.Count == 0) return;

            // рушим сверху вниз для красивого эффекта
            toBreak.Sort((a, b) => b.y.CompareTo(a.y));

            foreach (var bp in toBreak)
                _manager.Blocks.Break(bp, layer, BlockUpdateSource.System);
        }

        public void RegisterSystem(WorldManager manager)
        {
            _manager = manager;
            _manager.Blocks.Events.OnBlockBroken += BlockBreakMatcher;
        }

        public void Dispose()
        {
            if (_manager != null)
            {
                _manager.Blocks.Events.OnBlockBroken -= BlockBreakMatcher;
                _manager = null;
            }
        }
    }
}