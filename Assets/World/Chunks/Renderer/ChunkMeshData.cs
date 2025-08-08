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
        private readonly List<Color32> _colors = new List<Color32>();
        private readonly Dictionary<int, BlockIndex> _quadCoordMap = new Dictionary<int, BlockIndex>();

        private Mesh _mesh;
        private GameObject _parentGO;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private bool _isDirty;
        private int _quadCount;

        public BlockAtlasInfo BlockAtlas { get; }

        public ChunkMeshData(BlockAtlasInfo atlasInfo)
        {
            BlockAtlas = atlasInfo;
            _quadCount = 0;
        }

        public int AddQuadAt(BlockIndex index, ushort blockId, bool darkness = false)
        {
            int quadIndex = _quadCount++;
            int baseVertex = _vertices.Count;
            var uvRect = BlockAtlas.GetRect(blockId);
            var eps = BlockAtlas.Epsilon;

            _quadCoordMap[quadIndex] = index;

            // Vertices
            _vertices.Add(new Vector3(index.x,     index.y,     0));
            _vertices.Add(new Vector3(index.x + 1, index.y,     0));
            _vertices.Add(new Vector3(index.x + 1, index.y + 1, 0));
            _vertices.Add(new Vector3(index.x,     index.y + 1, 0));

            // Triangles
            _triangles.Add(baseVertex);
            _triangles.Add(baseVertex + 2);
            _triangles.Add(baseVertex + 1);
            _triangles.Add(baseVertex);
            _triangles.Add(baseVertex + 3);
            _triangles.Add(baseVertex + 2);

            // UVs
            _uvs.Add(new Vector2(uvRect.xMin + eps, uvRect.yMin + eps));
            _uvs.Add(new Vector2(uvRect.xMax - eps, uvRect.yMin + eps));
            _uvs.Add(new Vector2(uvRect.xMax - eps, uvRect.yMax - eps));
            _uvs.Add(new Vector2(uvRect.xMin + eps, uvRect.yMax - eps));

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
            var eps = BlockAtlas.Epsilon;
            var color = darkness ? DarknessColor : WhiteColor;

            // UVs
            _uvs[start + 0] = new Vector2(uvRect.xMin + eps, uvRect.yMin + eps);
            _uvs[start + 1] = new Vector2(uvRect.xMax - eps, uvRect.yMin + eps);
            _uvs[start + 2] = new Vector2(uvRect.xMax - eps, uvRect.yMax - eps);
            _uvs[start + 3] = new Vector2(uvRect.xMin + eps, uvRect.yMax - eps);

            // Colors
            for (int i = 0; i < 4; i++)
                _colors[start + i] = color;

            _isDirty = true;
        }

        public BlockIndex? RemoveQuadAt(int quadIndex)
        {
            int last = _quadCount - 1;
            if (quadIndex < 0 || quadIndex > last) return null;

            int removeStart = last * 4;
            bool moved = _quadCoordMap.TryGetValue(last, out var movedCoord);
            _quadCount--;

            if (quadIndex != last && moved)
            {
                int dst = quadIndex * 4;
                for (int i = 0; i < 4; i++)
                {
                    _vertices[dst + i] = _vertices[removeStart + i];
                    _uvs     [dst + i] = _uvs     [removeStart + i];
                    _colors  [dst + i] = _colors  [removeStart + i];
                }
                _quadCoordMap[quadIndex] = movedCoord;
            }

            _vertices.RemoveRange(removeStart, 4);
            _uvs     .RemoveRange(removeStart, 4);
            _colors  .RemoveRange(removeStart, 4);

            // Rebuild triangles
            RebuildTriangles(quadIndex);

            _quadCoordMap.Remove(last);
            _isDirty = true;

            return (quadIndex != last && moved) ? movedCoord : (BlockIndex?)null;
        }

        private void RebuildTriangles(int removedIndex)
        {
            _triangles.Clear();

            for (int i = 0; i < _quadCount; i++)
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
            _mesh.SetColors(_colors);
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();
            _isDirty = false;
        }
    }
}