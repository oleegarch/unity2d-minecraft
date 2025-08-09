using UnityEngine;
using World.Chunks.Generator.Steps;

namespace World.Chunks.Generator.Providers
{
    // Surface height provider with biome-based Perlin blending + caching
    public class SurfaceYProvider : ISurfaceHeightProvider, IChunkCacheStep
    {
        private readonly CacheComputationByX<int> _cacheHelper;
        private readonly IBiomeProvider _biomeProvider;
        private readonly float _blendDistance;
        private readonly float _biomeWidth;

        public SurfaceYProvider(
            IBiomeProvider biomeProvider,
            float biomeWidth,
            float blendDistance)
        {
            _cacheHelper = new CacheComputationByX<int>(ComputeSurfaceY);
            _biomeProvider = biomeProvider;
            _biomeWidth = biomeWidth;
            _blendDistance = blendDistance;
        }

        public void CacheComputation(RectInt rect, int seed) => _cacheHelper.CacheComputation(rect, seed);
        public int GetSurfaceY(int worldX, int seed) => _cacheHelper.GetValue(worldX, seed);

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