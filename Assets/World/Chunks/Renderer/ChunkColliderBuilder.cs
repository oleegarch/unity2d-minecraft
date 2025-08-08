using UnityEngine;
using System.Collections.Generic;
using World.Blocks;
using World.Chunks.BlocksStorage;

namespace World.Chunks
{
    public class ChunkColliderBuilder
    {
        private struct Edge
        {
            public Vector2 Start;
            public Vector2 End;

            public Edge(Vector2 s, Vector2 e)
            {
                Start = s;
                End = e;
            }
        }

        private readonly PolygonCollider2D _collider;
        private List<Vector2[]> _paths;
        private Chunk _chunk;

        public ChunkColliderBuilder(PolygonCollider2D collider)
        {
            _collider = collider;
            _paths = new List<Vector2[]>();
        }

        public ChunkColliderBuilder BuildCollider(Chunk chunk)
        {
            _chunk = chunk;
            
            Dispose();

            int size = chunk.Size;
            var edges = new List<Edge>(size * size * 4);

            bool IsFilled(int x, int y) => !chunk.GetBlock((byte)x, (byte)y, BlockLayer.Main).IsAir();

            // собрираем все рёбра, отделяющие заполненные клетки от пустых
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (!IsFilled(x, y))
                        continue;

                    // левая граница
                    if (x == 0 || !IsFilled(x - 1, y))
                        edges.Add(new Edge(
                            new Vector2(x, y + 1),
                            new Vector2(x, y)
                        ));

                    // нижняя граница
                    if (y == 0 || !IsFilled(x, y - 1))
                        edges.Add(new Edge(
                            new Vector2(x, y),
                            new Vector2(x + 1, y)
                        ));

                    // правая граница
                    if (x == size - 1 || !IsFilled(x + 1, y))
                        edges.Add(new Edge(
                            new Vector2(x + 1, y),
                            new Vector2(x + 1, y + 1)
                        ));

                    // верхняя граница
                    if (y == size - 1 || !IsFilled(x, y + 1))
                        edges.Add(new Edge(
                            new Vector2(x + 1, y + 1),
                            new Vector2(x, y + 1)
                        ));
                }
            }

            // собрираем рёбра в замкнутые контуры
            var edgeSet = new List<Edge>(edges);

            while (edgeSet.Count > 0)
            {
                // начинаем новый контур
                var first = edgeSet[0];
                edgeSet.RemoveAt(0);

                var path = new List<Vector2> { first.Start, first.End };
                Vector2 currentEnd = first.End;

                bool closed = false;
                while (!closed)
                {
                    bool foundNext = false;
                    for (int i = 0; i < edgeSet.Count; i++)
                    {
                        var e = edgeSet[i];
                        if (Vector2.Equals(e.Start, currentEnd))
                        {
                            // продолжаем идти по этому ребру
                            path.Add(e.End);
                            currentEnd = e.End;
                            edgeSet.RemoveAt(i);
                            foundNext = true;
                            break;
                        }
                    }

                    if (!foundNext)
                    {
                        // если не нашли продолжение, значит контур закрыт
                        closed = true;
                    }
                    else if (Vector2.Equals(path[0], currentEnd))
                    {
                        // вернулись в начало — контур замкнулся
                        closed = true;
                    }
                }

                // если последняя точка совпадает с первой, можно убрать дублирование
                if (path.Count > 1 && Vector2.Equals(path[0], path[path.Count - 1]))
                    path.RemoveAt(path.Count - 1);

                _paths.Add(path.ToArray());
            }

            return this;
        }
        public void ApplyCollider()
        {
            _collider.pathCount = _paths.Count;

            for (int i = 0; i < _paths.Count; i++)
            {
                _collider.SetPath(i, _paths[i]);
            }
        }

        public void AddSquare(BlockIndex blockIndex)
        {
            BuildCollider(_chunk);
            ApplyCollider();
        }
        public void RemoveSquare(BlockIndex blockIndex)
        {
            BuildCollider(_chunk);
            ApplyCollider();
        }

        public void Dispose()
        {
            _paths.Clear();
        }
    }
}