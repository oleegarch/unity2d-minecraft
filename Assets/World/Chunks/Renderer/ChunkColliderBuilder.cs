using System;
using System.Collections.Generic;
using R3;
using UnityEngine;
using World.Blocks;
using World.Chunks.Blocks;
using World.Blocks.Atlases;

namespace World.Chunks
{
    public class ChunkColliderBuilder : IDisposable
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
        private readonly BlockDatabase _blockDatabase;

        private List<Vector2[]> _paths;
        private Chunk _chunk;
        private bool _needRefresh;

        public ChunkColliderBuilder(PolygonCollider2D collider, BlockDatabase blockDatabase, BlockAtlasDatabase blockAtlasDatabase)
        {
            _collider = collider;
            _blockDatabase = blockDatabase;
            _paths = new List<Vector2[]>();
        }

        public void Refresh()
        {
            if (_needRefresh)
            {
                ApplyCollider();
            }
        }

        public ChunkColliderBuilder BuildCollider(Chunk chunk)
        {
            _chunk = chunk;
            Dispose();
            SubscribeToChunkChanges();

            int size = chunk.Size;
            var edges = new List<Edge>(size * size * 4);

            var hasCustom = new bool[size, size];
            var spriteRects = new Rect[size, size];

            for (int xi = 0; xi < size; xi++)
            {
                for (int yi = 0; yi < size; yi++)
                {
                    var block = chunk.Blocks.Get(new BlockIndex((byte)xi, (byte)yi), BlockLayer.Main);
                    if (block.IsAir) continue;

                    var info = _blockDatabase.Get(block.Id);
                    if (info.HasCustomCollider)
                    {
                        spriteRects[xi, yi] = info.VisibleSpriteRect;
                        hasCustom[xi, yi] = true;
                    }
                }
            }

            bool IsBlockAir(int x, int y)
            {
                if (x < 0 || y < 0 || x >= size || y >= size) return true;
                return chunk.Blocks.Get(new BlockIndex((byte)x, (byte)y), BlockLayer.Main).IsAir;
            }
            bool HasCustomCollider(int x, int y, out Rect spriteRect)
            {
                if (x < 0 || y < 0 || x >= size || y >= size)
                {
                    spriteRect = Rect.zero;
                    return false;
                }

                if (hasCustom[x, y])
                {
                    spriteRect = spriteRects[x, y];
                    return true;
                }

                spriteRect = Rect.zero;
                return false;
            }

            // собираем все рёбра, отделяющие заполненные клетки от пустых (для сетки 1x1)
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    if (IsBlockAir(x, y))
                        continue;

                    // если у клетки кастомный коллайдер — добавляем её кастомный путь и пропускаем сетку для этой клетки
                    if (HasCustomCollider(x, y, out Rect spriteRect))
                    {
                        float x0 = x + spriteRect.x;
                        float y0 = y + spriteRect.y;
                        float x1 = x0 + spriteRect.width;
                        float y1 = y0 + spriteRect.height;

                        _paths.Add(new Vector2[]
                        {
                            new Vector2(x0, y0),
                            new Vector2(x1, y0),
                            new Vector2(x1, y1),
                            new Vector2(x0, y1)
                        });

                        continue;
                    }

                    // Для остальных клеток (обычных 1x1) — добавляем ребра
                    if (x == 0 || IsBlockAir(x - 1, y) || HasCustomCollider(x - 1, y, out _))
                        edges.Add(new Edge(
                            new Vector2(x, y + 1),
                            new Vector2(x, y)
                        ));

                    if (y == 0 || IsBlockAir(x, y - 1) || HasCustomCollider(x, y - 1, out _))
                        edges.Add(new Edge(
                            new Vector2(x, y),
                            new Vector2(x + 1, y)
                        ));

                    if (x == size - 1 || IsBlockAir(x + 1, y) || HasCustomCollider(x + 1, y, out _))
                        edges.Add(new Edge(
                            new Vector2(x + 1, y),
                            new Vector2(x + 1, y + 1)
                        ));

                    if (y == size - 1 || IsBlockAir(x, y + 1) || HasCustomCollider(x, y + 1, out _))
                        edges.Add(new Edge(
                            new Vector2(x + 1, y + 1),
                            new Vector2(x, y + 1)
                        ));
                }
            }

            // собираем рёбра в замкнутые контуры
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

            _needRefresh = true;
            
            return this;
        }

        public ChunkColliderBuilder ApplyCollider()
        {
            _collider.pathCount = _paths.Count;

            for (int i = 0; i < _paths.Count; i++)
            {
                _collider.SetPath(i, _paths[i]);
            }
            
            _needRefresh = false;

            return this;
        }

        public void AddSquare(BlockIndex blockIndex)
        {
            BuildCollider(_chunk);
        }
        public void RemoveSquare(BlockIndex blockIndex)
        {
            BuildCollider(_chunk);
        }

        private readonly List<IDisposable> _subscriptions = new();
        private void SubscribeToChunkChanges()
        {
            _subscriptions.Add(_chunk.Events.BlockSet.Subscribe(be => AddSquare(be.Index)));
            _subscriptions.Add(_chunk.Events.BlockBroken.Subscribe(be => RemoveSquare(be.Index)));
        }
        private void DisposeSubscriptions()
        {
            foreach (var sub in _subscriptions) sub.Dispose();
            _subscriptions.Clear();
        }
        public void Dispose()
        {
            DisposeSubscriptions();
            _paths.Clear();
        }
    }
}