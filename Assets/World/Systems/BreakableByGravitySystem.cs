using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using World.Blocks;
using World.Chunks;

namespace World.Systems
{
    public class BreakableByGravitySystem : IWorldSystem
    {
        private WorldManager _manager;
        private IDisposable _subscription;

        // есть ли у узла "горизонтальная" цепочка до узла с опорой снизу?
        private bool HasSupportPathToGround(WorldPosition start, BlockLayer layer, HashSet<WorldPosition> planned)
        {
            var q = new Queue<WorldPosition>();
            var seen = new HashSet<WorldPosition>();
            q.Enqueue(start);

            while (q.Count > 0)
            {
                var p = q.Dequeue();
                if (!seen.Add(p)) continue;
                if (planned.Contains(p)) continue;

                var b = _manager.Blocks.GetSimilar(p, layer, out BlockLayer currentLayer);
                if (b.IsAir) continue;

                var below = p + Vector2Int.down;
                if (!planned.Contains(below))
                {
                    var bb = _manager.Blocks.GetSimilar(below, layer, out currentLayer);
                    if (!bb.IsAir)
                        return true;
                }

                var left = p + Vector2Int.left;
                var right = p + Vector2Int.right;

                var lb = _manager.Blocks.GetSimilar(left, layer, out currentLayer);
                if (!lb.IsAir && !planned.Contains(left)) q.Enqueue(left);

                var rb = _manager.Blocks.GetSimilar(right, layer, out currentLayer);
                if (!rb.IsAir && !planned.Contains(right)) q.Enqueue(right);
            }

            return false;
        }

        private void OnBlockBrokenByPlayer(WorldBlockEvent e)
        {
            var start = e.Position;
            var layer = e.Layer;

            var toBreak = new List<(WorldPosition position, BlockLayer layer)>();
            var planned = new HashSet<WorldPosition> { start };

            int version = 0;
            var processedAtVersion = new Dictionary<WorldPosition, int>();
            var queue = new Queue<WorldPosition>();
            queue.Enqueue(start + Vector2Int.up);

            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();

                if (processedAtVersion.TryGetValue(pos, out var v) && v == version)
                    continue;
                processedAtVersion[pos] = version;

                var cur = _manager.Blocks.GetSimilar(pos, layer, out BlockLayer currentLayer);
                if (cur.IsAir) continue;

                var info = _manager.EnvironmentAccessor.BlockDatabase.Get(cur.Id);
                if (info == null) continue;
                if (!info.BreakableByGravity) continue;

                if (HasSupportPathToGround(pos, layer, planned)) continue;

                if (planned.Add(pos))
                {
                    toBreak.Add((pos, currentLayer));
                    version++;

                    queue.Enqueue(pos + Vector2Int.up);
                    queue.Enqueue(pos + Vector2Int.left);
                    queue.Enqueue(pos + Vector2Int.right);
                }
            }

            foreach (var data in toBreak)
                _manager.Blocks.Break(data.position, data.layer, BlockUpdateSource.System);
        }

        public void RegisterSystem(WorldManager manager)
        {
            _manager = manager;
            // Подписка на R3 Subject
            _subscription = _manager.Events.BlockBrokenByPlayer.Subscribe(OnBlockBrokenByPlayer);
        }

        public void Dispose()
        {
            _subscription?.Dispose();
            _subscription = null;
            _manager = null;
        }
    }
}