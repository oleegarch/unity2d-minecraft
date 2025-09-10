using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Chunks.Generator.Steps;

namespace World.Chunks.Generator.Procedural
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
        private class BiomeRange
        {
            public Biome Biome;
            public RangeInt Range;
        }

        private readonly List<BiomeRange> _ranges = new();
        private readonly List<Biome> _biomes;
        private readonly int _biomeWidth;
        private readonly int _seed;

        public BiomeProvider(List<Biome> biomes, int biomeWidth, int seed)
        {
            _biomes = biomes;
            _biomeWidth = biomeWidth;
            _seed = seed;
        }

        public void CacheComputation(RectInt rect)
        {
            _ranges.Clear();

            for (int startX = GetStartX(rect.xMin); startX < rect.xMax; startX += _biomeWidth)
            {
                _ranges.Add(new BiomeRange
                {
                    Biome = ComputeBiomeByStartX(startX),
                    Range = new RangeInt(startX, _biomeWidth)
                });
            }
        }
        public Biome GetBiome(int worldX)
        {
            BiomeRange range = _ranges.FirstOrDefault(br => br.Range.start <= worldX && br.Range.end > worldX);
            if (range != null) return range.Biome;

            return ComputeBiome(worldX);
        }
        public int GetStartX(int worldX)
        {
            float ratio = worldX / (float)_biomeWidth;
            return Mathf.FloorToInt(ratio) * _biomeWidth;
        }
        public Biome ComputeBiome(int worldX)
        {
            int startX = GetStartX(worldX);
            return ComputeBiomeByStartX(startX);
        }
        public Biome ComputeBiomeByStartX(int startX)
        {
            var rand = new System.Random(_seed + startX);
            return _biomes[rand.Next(_biomes.Count)];
        }
    }
}