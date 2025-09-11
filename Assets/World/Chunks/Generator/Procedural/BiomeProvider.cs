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
        private readonly List<Biome> _biomes;
        private readonly byte _chunkSize;
        private readonly int _biomeWidth;
        private readonly int _seed;

        private List<BiomeRange> _rangesByVisible = new();
        private List<BiomeRange> _rangesByIndexes = new();
        private List<BiomeRange> _ranges = new();

        public BiomeProvider(List<Biome> biomes, byte chunkSize, int biomeWidth, int seed)
        {
            _chunkSize = chunkSize;
            _biomes = biomes;
            _biomeWidth = biomeWidth;
            _seed = seed;
        }
        
        #region Вычисление биома
        public Biome GetBiome(int worldX)
        {
            Biome biome = FindBiomeInRanges(worldX);
            if (biome != null) return biome;

            Debug.Log($"GetBiome cached not found!");

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
        #endregion

        #region Промежутки биомов
        public List<BiomeRange> GetBiomeRanges(int fromX, int toX)
        {
            List<BiomeRange> ranges = new();

            for (int startX = GetStartX(fromX); startX <= toX; startX += _biomeWidth)
            {
                ranges.Add(new BiomeRange
                {
                    Biome = ComputeBiomeByStartX(startX),
                    Range = new RangeInt(startX, _biomeWidth)
                });
            }

            return ranges;
        }
        #endregion
        
        #region Кеширование
        /// <summary>
        /// Ищет Biome для worldX через бинарный поиск.
        /// </summary>
        public Biome FindBiomeInRanges(int worldX)
        {
            int left = 0;
            int right = _ranges.Count - 1;

            while (left <= right)
            {
                int mid = (left + right) / 2;
                BiomeRange range = _ranges[mid];

                if (worldX < range.Range.start)
                {
                    right = mid - 1;
                }
                else if (worldX >= range.Range.end)
                {
                    left = mid + 1;
                }
                else
                {
                    return range.Biome; // найден диапазон
                }
            }

            return null; // ничего не нашли
        }
        public void CacheComputation(RectInt rect)
        {
            CacheComputation(rect.xMin, rect.xMax, _rangesByVisible);
        }
        public void CacheComputation(HashSet<ChunkIndex> indexes)
        {
            int fromX = indexes.Select(index => index.x * _chunkSize).Min();
            int toX = indexes.Select(index => index.x * _chunkSize + _chunkSize - 1).Max();
            CacheComputation(fromX, toX, _rangesByIndexes);
        }
        public void CacheComputation(int fromX, int toX, List<BiomeRange> removeIn)
        {
            List<BiomeRange> ranges = GetBiomeRanges(fromX, toX);
            removeIn.Clear();
            removeIn.AddRange(ranges);
            RebuildRanges();
        }
        public void RebuildRanges()
        {
            _ranges.Clear();
            _ranges.AddRange(_rangesByVisible);
            _ranges.AddRange(_rangesByIndexes);

            // сортируем по старту, чтобы бинарный поиск работал корректно
            _ranges.Sort((a, b) => a.Range.start.CompareTo(b.Range.start));
        }
        #endregion
    }
}