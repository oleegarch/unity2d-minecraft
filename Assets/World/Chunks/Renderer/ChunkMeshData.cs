using UnityEngine;
using System.Collections.Generic;
using World.Blocks;
using World.Blocks.Atlases;

namespace World.Chunks
{
    public class ChunkMeshData
    {
        private static readonly Color32 DarknessColor = new Color32(20, 20, 20, 255);
        private static readonly Color32 WhiteColor = new Color32(255, 255, 255, 255);

        private readonly List<Vector3> _vertices = new List<Vector3>();
        private readonly List<int> _triangles = new List<int>();
        private readonly List<Vector2> _uvs = new List<Vector2>();
        private readonly List<Vector4> _uvRects = new List<Vector4>();
        private readonly List<Color32> _colors = new List<Color32>();
        private readonly Dictionary<int, BlockIndex> _quadCoordMap = new Dictionary<int, BlockIndex>();

        private Mesh _mesh;
        private GameObject _parentGO;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private bool _isDirty;

        public BlockAtlasInfo BlockAtlas { get; }
        public int QuadCount { get; private set; }

        public ChunkMeshData(BlockAtlasInfo atlasInfo)
        {
            BlockAtlas = atlasInfo;
            QuadCount = 0;
        }

        public int AddQuadAt(BlockIndex index, ushort blockId, bool darkness = false)
        {
            int quadIndex = QuadCount++;
            int baseVertex = _vertices.Count;
            var uvRect = BlockAtlas.GetRect(blockId);

            // 1) Вершины
            var spriteSize = BlockAtlas.GetSpriteSize(blockId);
            var quadVerts = ComputeQuadVerticesForIndex(index, spriteSize);
            _vertices.Add(quadVerts[0]);
            _vertices.Add(quadVerts[1]);
            _vertices.Add(quadVerts[2]);
            _vertices.Add(quadVerts[3]);

            // 2) Треугольники — как было
            _triangles.Add(baseVertex);
            _triangles.Add(baseVertex + 2);
            _triangles.Add(baseVertex + 1);
            _triangles.Add(baseVertex);
            _triangles.Add(baseVertex + 3);
            _triangles.Add(baseVertex + 2);

            // 3) UV0 — как у тебя (хочешь — без eps)
            _uvs.Add(new Vector2(uvRect.xMin, uvRect.yMin));
            _uvs.Add(new Vector2(uvRect.xMax, uvRect.yMin));
            _uvs.Add(new Vector2(uvRect.xMax, uvRect.yMax));
            _uvs.Add(new Vector2(uvRect.xMin, uvRect.yMax));

            // 4) UV1: границы прямоугольника тайла (одинаковые для всех 4 вершин квада)
            var bounds = new Vector4(uvRect.xMin, uvRect.yMin, uvRect.xMax, uvRect.yMax);
            _uvRects.Add(bounds);
            _uvRects.Add(bounds);
            _uvRects.Add(bounds);
            _uvRects.Add(bounds);

            // Colors
            var color = darkness ? DarknessColor : WhiteColor;
            for (int i = 0; i < 4; i++) _colors.Add(color);

            _isDirty = true;

            return quadIndex;
        }

        public void UpdateQuadAt(int quadIndex, ushort blockId, bool darkness = false)
        {
            int start = quadIndex * 4;
            var uvRect = BlockAtlas.GetRect(blockId);
            var color = darkness ? DarknessColor : WhiteColor;

            // UV0
            _uvs[start + 0] = new Vector2(uvRect.xMin, uvRect.yMin);
            _uvs[start + 1] = new Vector2(uvRect.xMax, uvRect.yMin);
            _uvs[start + 2] = new Vector2(uvRect.xMax, uvRect.yMax);
            _uvs[start + 3] = new Vector2(uvRect.xMin, uvRect.yMax);

            // UV1 (границы)
            var bounds = new Vector4(uvRect.xMin, uvRect.yMin, uvRect.xMax, uvRect.yMax);
            _uvRects[start + 0] = bounds;
            _uvRects[start + 1] = bounds;
            _uvRects[start + 2] = bounds;
            _uvRects[start + 3] = bounds;

            // Colors
            for (int i = 0; i < 4; i++)
                _colors[start + i] = color;

            // Обновляем вершины тоже (если спрайт другой размера)
            if (!_quadCoordMap.TryGetValue(quadIndex, out var index))
            {
                _isDirty = true;
                return;
            }

            var spriteSize = BlockAtlas.GetSpriteSize(blockId);
            var quadVerts = ComputeQuadVerticesForIndex(index, spriteSize);
            _vertices[start + 0] = quadVerts[0];
            _vertices[start + 1] = quadVerts[1];
            _vertices[start + 2] = quadVerts[2];
            _vertices[start + 3] = quadVerts[3];

            _isDirty = true;
        }

        public BlockIndex? RemoveQuadAt(int quadIndex)
        {
            int last = QuadCount - 1;
            if (quadIndex < 0 || quadIndex > last) return null;

            int removeStart = last * 4;
            bool moved = _quadCoordMap.TryGetValue(last, out var movedCoord);
            QuadCount--;

            if (quadIndex != last && moved)
            {
                int dst = quadIndex * 4;
                for (int i = 0; i < 4; i++)
                {
                    _vertices[dst + i] = _vertices[removeStart + i];
                    _uvs[dst + i] = _uvs[removeStart + i];
                    _uvRects[dst + i] = _uvRects[removeStart + i]; // <-- добавь это
                    _colors[dst + i] = _colors[removeStart + i];
                }
                _quadCoordMap[quadIndex] = movedCoord;
            }

            _vertices.RemoveRange(removeStart, 4);
            _uvs.RemoveRange(removeStart, 4);
            _uvRects.RemoveRange(removeStart, 4); // <-- добавь это
            _colors.RemoveRange(removeStart, 4);

            // Rebuild triangles
            RebuildTriangles(quadIndex);

            _quadCoordMap.Remove(last);
            _isDirty = true;

            return (quadIndex != last && moved) ? movedCoord : (BlockIndex?)null;
        }

        private void RebuildTriangles(int removedIndex)
        {
            _triangles.Clear();

            for (int i = 0; i < QuadCount; i++)
            {
                int baseV = i * 4;
                _triangles.Add(baseV);
                _triangles.Add(baseV + 2);
                _triangles.Add(baseV + 1);
                _triangles.Add(baseV);
                _triangles.Add(baseV + 3);
                _triangles.Add(baseV + 2);
            }
        }

        public GameObject ApplyMesh()
        {
            if (_parentGO == null)
            {
                _parentGO = new GameObject(BlockAtlas.Category.ToString());
                _mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
                _meshFilter = _parentGO.AddComponent<MeshFilter>();
                _meshRenderer = _parentGO.AddComponent<MeshRenderer>();
                _meshFilter.mesh = _mesh;
                _meshRenderer.sharedMaterial = BlockAtlas.Material;
            }

            RefreshMesh();

            return _parentGO;
        }

        public void RefreshMesh()
        {
            if (!_isDirty || _mesh == null) return;
            _mesh.Clear();
            _mesh.SetVertices(_vertices);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.SetUVs(0, _uvs);
            _mesh.SetUVs(1, _uvRects);
            _mesh.SetColors(_colors);
            _mesh.RecalculateBounds();
            _isDirty = false;
        }

        private Vector3[] ComputeQuadVerticesForIndex(BlockIndex index, Rect sizeUnits)
        {
            float x0 = index.x + sizeUnits.x;
            float y0 = index.y + sizeUnits.y;
            float x1 = x0 + sizeUnits.width;
            float y1 = y0 + sizeUnits.height;

            return new[]
            {
                new Vector3(x0, y0, 0f),
                new Vector3(x1, y0, 0f),
                new Vector3(x1, y1, 0f),
                new Vector3(x0, y1, 0f)
            };
        }
    }
}