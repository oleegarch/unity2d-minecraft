using System.Collections.Generic;
using UnityEngine;

namespace World.Chunks.Generator.Procedural
{
    public class BiomeRange
    {
        public Biome Biome;
        public RangeInt Range;
    }
    public interface IBiomeProvider
    {
        public Biome GetBiome(int worldX);
        public List<BiomeRange> GetBiomeRanges(int fromX, int toX);
    }
}