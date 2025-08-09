using System;
using System.Collections.Generic;
using UnityEngine;
using World.Chunks.Generator.Steps;

namespace World.Chunks.Generator.Providers
{
    [Serializable]
    public class Biome
    {
        [Tooltip("название биома для последующей идентификации")]
        public string Name;

        [Tooltip("первый блок биома (трава, снег)")]
        public string SurfaceBlockName;

        [Tooltip("чем больше, тем менее ровная поверхность")]
        public float SurfaceScale = 0.05f;

        [Tooltip("средний уровень поверхности")]
        public int SurfaceBaseHeight = 0;
        
        [Tooltip("насколько может колебаться поверхность")]
        public int SurfaceHeightRange = 20;

        [Tooltip("нижние блоки биома (земля, песок)")]
        public string DepthBlockName;

        [Tooltip("глубина нижних блоков биома")]
        public int Depth = 15;
    }

    // Biome provider implementation
    public class BiomeProvider : IBiomeProvider, IChunkCacheStep
    {
        private readonly CacheComputationByX<Biome> _cacheHelper;
        private readonly List<Biome> _biomes;
        private readonly float _biomeWidth;

        public BiomeProvider(List<Biome> biomes, float biomeWidth)
        {
            _cacheHelper = new CacheComputationByX<Biome>(MakeKey, ComputeBiome);
            _biomes = biomes;
            _biomeWidth = biomeWidth;
        }

        private int MakeKey(int worldX, int seed) => (worldX << 16) ^ seed;
        public void CacheComputation(RectInt rect, int seed) => _cacheHelper.CacheComputation(rect, seed);
        public Biome GetBiome(int worldX, int seed) => _cacheHelper.GetValue(worldX, seed);

        private Biome ComputeBiome(int worldX, int seed)
        {
            int zone = Mathf.FloorToInt(worldX / _biomeWidth);
            var rand = new System.Random(seed + zone);
            return _biomes[rand.Next(_biomes.Count)];
        }
    }
}