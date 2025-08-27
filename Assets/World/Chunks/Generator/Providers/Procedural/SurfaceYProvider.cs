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
        private readonly int _seed;

        public SurfaceYProvider(
            IBiomeProvider biomeProvider,
            float biomeWidth,
            float blendDistance,
            int seed)
        {
            _cacheHelper = new CacheComputationByX<int>(ComputeSurfaceY, seed);
            _biomeProvider = biomeProvider;
            _biomeWidth = biomeWidth;
            _blendDistance = blendDistance;
            _seed = seed;
        }

        public void CacheComputation(RectInt rect) => _cacheHelper.CacheComputation(rect);
        public int GetSurfaceY(int worldX) => _cacheHelper.GetValue(worldX);

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
    }
}