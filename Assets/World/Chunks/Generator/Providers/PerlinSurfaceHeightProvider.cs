using UnityEngine;

namespace World.Chunks.Generator.Providers
{
    // Surface height provider with biome-based Perlin blending + caching
    public class PerlinSurfaceHeightProvider : ISurfaceHeightProvider
    {
        private readonly IBiomeProvider _biomeProvider;
        private readonly float _blendDistance;
        private readonly float _biomeWidth;

        // Кеш результатов: key = (worldX << 16) ^ seed
        private readonly ComputationCache<int, int> _cache = new();

        public PerlinSurfaceHeightProvider(
            IBiomeProvider biomeProvider,
            float biomeWidth,
            float blendDistance)
        {
            _biomeProvider = biomeProvider;
            _biomeWidth = biomeWidth;
            _blendDistance = blendDistance;
        }

        public int GetSurfaceY(int worldX, int seed)
        {
            int key = (worldX << 16) ^ seed;
            return _cache.GetOrAdd(key, _ => CalculateSurfaceY(worldX, seed));
        }

        private int CalculateSurfaceY(int worldX, int seed)
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