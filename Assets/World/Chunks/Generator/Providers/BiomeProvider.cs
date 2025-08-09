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
        private readonly List<Biome> _biomes;
        private readonly float _biomeWidth;

        // Кеш высот: key = (worldX << 16) ^ seed
        private readonly Dictionary<int, Biome> _biomeCache = new();
        private RectInt? _prevRect;

        public BiomeProvider(List<Biome> biomes, float biomeWidth)
        {
            _biomes = biomes;
            _biomeWidth = biomeWidth;
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
                        _biomeCache.Remove(MakeKey(x, seed));
                }
                // Удаляем справа
                if (rect.xMax < prev.xMax)
                {
                    for (int x = rect.xMax + 1; x <= prev.xMax; x++)
                        _biomeCache.Remove(MakeKey(x, seed));
                }

                // Добавляем слева
                if (rect.xMin < prev.xMin)
                {
                    for (int x = rect.xMin; x < prev.xMin; x++)
                        _biomeCache[MakeKey(x, seed)] = ComputeBiome(x, seed);
                }
                // Добавляем справа
                if (rect.xMax > prev.xMax)
                {
                    for (int x = prev.xMax + 1; x <= rect.xMax; x++)
                        _biomeCache[MakeKey(x, seed)] = ComputeBiome(x, seed);
                }
            }
            else
            {
                // Первое заполнение
                for (int x = rect.xMin; x <= rect.xMax; x++)
                    _biomeCache[MakeKey(x, seed)] = ComputeBiome(x, seed);
            }

            _prevRect = rect;
        }

        public Biome GetBiome(int worldX, int seed)
        {
            int key = MakeKey(worldX, seed);
            if (_biomeCache.TryGetValue(key, out var cachedY))
                return cachedY;

            Biome result = ComputeBiome(worldX, seed);
            _biomeCache[key] = result;
            return result;
        }
        private Biome ComputeBiome(int worldX, int seed)
        {
            int zone = Mathf.FloorToInt(worldX / _biomeWidth);
            var rand = new System.Random(seed + zone);
            return _biomes[rand.Next(_biomes.Count)];
        }
    }
}