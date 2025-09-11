using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Chunks.Generator.Steps;

namespace World.Chunks.Generator.Procedural
{
    // Surface height provider with biome-based Perlin blending + caching
    public class SurfaceYProvider : ISurfaceYProvider, IChunkCacheStep
    {
        private readonly IBiomeProvider _biomeProvider;
        private readonly float _blendDistance;
        private readonly float _biomeWidth;
        private readonly byte _chunkSize;
        private readonly int _seed;

        private Dictionary<int, int> _cacheByVisible = new();
        private Dictionary<int, int> _cacheByIndexes = new();

        public SurfaceYProvider(
            IBiomeProvider biomeProvider,
            byte chunkSize,
            float biomeWidth,
            float blendDistance,
            int seed)
        {
            _biomeProvider = biomeProvider;
            _chunkSize = chunkSize;
            _biomeWidth = biomeWidth;
            _blendDistance = blendDistance;
            _seed = seed;
        }

        public int GetSurfaceY(int worldX)
        {
            int cachedSurfaceY;

            if (_cacheByVisible.TryGetValue(worldX, out cachedSurfaceY) || _cacheByIndexes.TryGetValue(worldX, out cachedSurfaceY))
                return cachedSurfaceY;

            return ComputeSurfaceY(worldX);
        }

        public int ComputeSurfaceY(int worldX)
        {
            Biome biome = _biomeProvider.GetBiome(worldX);
            float rawNoise = CalculateHeightNoise(worldX, biome);
            float leftXToNextBiome = _biomeWidth - worldX % _biomeWidth;
            int biomeWidthInt = (int)_biomeWidth;

            // Blending at biome boundaries
            if (leftXToNextBiome < _blendDistance)
            {
                int adjacentX = Mathf.FloorToInt(worldX / _biomeWidth + 1) * biomeWidthInt;
                Biome adjacentBiome = _biomeProvider.GetBiome(adjacentX);
                float adjacentNoise = CalculateHeightNoise(adjacentX, adjacentBiome);
                float blendFactor = leftXToNextBiome / _blendDistance;

                float blendedHeight = Mathf.Lerp(adjacentNoise, rawNoise, blendFactor);
                return Mathf.FloorToInt(blendedHeight);
            }

            return Mathf.FloorToInt(rawNoise);
        }

        private float CalculateHeightNoise(int worldX, Biome biome)
        {
            float samplePositionX = (worldX + _seed) * biome.SurfaceScale;
            float perlinValue = Mathf.PerlinNoise(samplePositionX, 0f);

            return perlinValue * biome.SurfaceHeightRange + biome.SurfaceBaseHeight;
        }

        #region Кеширование
        public void CacheComputation(RectInt rect)
        {
            CacheComputation(rect.xMin, rect.xMax, _cacheByVisible);
        }
        public void CacheComputation(HashSet<ChunkIndex> indexes)
        {
            int fromX = indexes.Select(index => index.x * _chunkSize).Min();
            int toX = indexes.Select(index => index.x * _chunkSize + _chunkSize - 1).Max();
            CacheComputation(fromX, toX, _cacheByIndexes);
        }
        public void CacheComputation(int fromX, int toX, Dictionary<int, int> updateIt)
        {
            updateIt.Clear();

            for (int worldX = fromX; worldX <= toX; worldX++)
                updateIt[worldX] = ComputeSurfaceY(worldX);
        }
        #endregion
    }
}