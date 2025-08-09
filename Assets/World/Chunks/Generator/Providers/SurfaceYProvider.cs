using UnityEngine;
using System.Collections.Generic;
using World.Chunks.Generator.Steps;

namespace World.Chunks.Generator.Providers
{
    // Surface height provider with biome-based Perlin blending + caching
    public class SurfaceYProvider : ISurfaceHeightProvider, IChunkCacheStep
    {
        private readonly IBiomeProvider _biomeProvider;
        private readonly float _blendDistance;
        private readonly float _biomeWidth;

        // Кеш высот: key = (worldX << 16) ^ seed
        private readonly Dictionary<int, int> _surfaceYCache = new();
        private RectInt? _prevRect;

        public SurfaceYProvider(
            IBiomeProvider biomeProvider,
            float biomeWidth,
            float blendDistance)
        {
            _biomeProvider = biomeProvider;
            _biomeWidth = biomeWidth;
            _blendDistance = blendDistance;
        }

        private int MakeKey(int worldX, int seed) => (worldX << 16) ^ seed;

        public void CacheComputation(RectInt rect, int seed)
        {
            if (_prevRect.HasValue)
            {
                var prev = _prevRect.Value;

                // Удаляем слева
                if (rect.xMin > prev.xMin)
                {
                    for (int x = prev.xMin; x < rect.xMin; x++)
                        _surfaceYCache.Remove(MakeKey(x, seed));
                }
                // Удаляем справа
                if (rect.xMax < prev.xMax)
                {
                    for (int x = rect.xMax + 1; x <= prev.xMax; x++)
                        _surfaceYCache.Remove(MakeKey(x, seed));
                }

                // Добавляем слева
                if (rect.xMin < prev.xMin)
                {
                    for (int x = rect.xMin; x < prev.xMin; x++)
                        _surfaceYCache[MakeKey(x, seed)] = ComputeSurfaceY(x, seed);
                }
                // Добавляем справа
                if (rect.xMax > prev.xMax)
                {
                    for (int x = prev.xMax + 1; x <= rect.xMax; x++)
                        _surfaceYCache[MakeKey(x, seed)] = ComputeSurfaceY(x, seed);
                }
            }
            else
            {
                // Первое заполнение
                for (int x = rect.xMin; x <= rect.xMax; x++)
                    _surfaceYCache[MakeKey(x, seed)] = ComputeSurfaceY(x, seed);
            }

            _prevRect = rect;
        }

        public int GetSurfaceY(int worldX, int seed)
        {
            int key = MakeKey(worldX, seed);
            if (_surfaceYCache.TryGetValue(key, out var cachedY))
                return cachedY;

            int result = ComputeSurfaceY(worldX, seed);
            _surfaceYCache[key] = result;
            return result;
        }

        public int ComputeSurfaceY(int worldX, int seed)
        {
            Biome biome = _biomeProvider.GetBiome(worldX, seed);
            float rawNoise = CalculateHeightNoise(worldX, seed, biome);
            float leftXToNextBiome = _biomeWidth - worldX % _biomeWidth;
            int biomeWidthInt = (int)_biomeWidth;

            // Blending at biome boundaries
            if (leftXToNextBiome < _blendDistance)
            {
                int adjacentX = Mathf.FloorToInt(worldX / _biomeWidth + 1) * biomeWidthInt;
                Biome adjacentBiome = _biomeProvider.GetBiome(adjacentX, seed);
                float adjacentNoise = CalculateHeightNoise(adjacentX, seed, adjacentBiome);
                float blendFactor = leftXToNextBiome / _blendDistance;

                float blendedHeight = Mathf.Lerp(adjacentNoise, rawNoise, blendFactor);
                return Mathf.FloorToInt(blendedHeight);
            }

            return Mathf.FloorToInt(rawNoise);
        }

        private float CalculateHeightNoise(int worldX, int seed, Biome biome)
        {
            float samplePositionX = (worldX + seed) * biome.SurfaceScale;
            float perlinValue = Mathf.PerlinNoise(samplePositionX, 0f);

            return perlinValue * biome.SurfaceHeightRange + biome.SurfaceBaseHeight;
        }
    }
}