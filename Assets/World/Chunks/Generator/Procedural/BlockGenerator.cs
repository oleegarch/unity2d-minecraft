using System;
using System.Collections.Generic;
using UnityEngine;
using World.Blocks;

namespace World.Chunks.Generator.Procedural
{
    [Serializable]
    public class CaveLevel
    {
        [Tooltip("какой блок использовать (stone, basalt и т.д.)")]
        public string BlockName;

        [Tooltip("высота уровня (например, 60 блоков)")]
        public int Height;

        [Tooltip("правила спавна руд")]
        public List<OreSpawnRule> Ores;
    }

    [Serializable]
    public class OreSpawnRule
    {
        [Tooltip("название блока руды")]
        public string OreBlockName;

        [Tooltip("чем больше, тем массивнее жилки этой руды")]
        public float Threshold = 0.8f;

        [Tooltip("чем больше, тем больше размытие")]
        public float Scale = 0.2f;
    }

    // Procedural Earth block generator: surface, depth, cave, ores
    public class BlockGenerator : IProceduralBlockProvider
    {
        private readonly BlockDatabase _blockDatabase;
        private readonly IBiomeProvider _biomeProvider;
        private readonly ISurfaceYProvider _surfaceHeightProvider;
        private readonly List<CaveLevel> _caveLevelList;
        private readonly int _seed;
        public BlockGenerator(
            BlockDatabase blockDatabase,
            IBiomeProvider biomeProvider,
            ISurfaceYProvider surfaceHeightProvider,
            List<CaveLevel> caveLevelList,
            int seed)
        {
            _blockDatabase = blockDatabase;
            _biomeProvider = biomeProvider;
            _surfaceHeightProvider = surfaceHeightProvider;
            _caveLevelList = caveLevelList;
            _seed = seed;
        }

        public (ushort mainId, ushort backgroundId) GenerateBlock(
            int worldX,
            int worldY)
        {
            var biome = _biomeProvider.GetBiome(worldX);
            int surfaceY = _surfaceHeightProvider.GetSurfaceY(worldX);
            int caveStartY = surfaceY - biome.Depth;

            // Chest test
            if (worldX == 1 && worldY - surfaceY == 1)
            {
                ushort chestId = _blockDatabase.GetId("chest");
                return (chestId, Block.AirId);
            }

            // Above surface
            if (worldY > surfaceY)
            {
                return (Block.AirId, Block.AirId);
            }

            // Surface layer
            if (worldY == surfaceY)
            {
                ushort surfaceId = _blockDatabase.GetId(biome.SurfaceBlockName);
                return (surfaceId, surfaceId);
            }

            // Depth layer above caves
            if (worldY > caveStartY)
            {
                ushort depthId = _blockDatabase.GetId(biome.DepthBlockName);
                return (depthId, depthId);
            }

            // Cave and ore layers
            int relativeDepth = Mathf.Abs(worldY - caveStartY);
            int cumulativeHeight = 0;

            foreach (var caveLevel in _caveLevelList)
            {
                cumulativeHeight += caveLevel.Height;

                if (relativeDepth < cumulativeHeight)
                {
                    ushort stoneId = _blockDatabase.GetId(caveLevel.BlockName);
                    ushort backgroundId = stoneId;

                    foreach (var oreEntry in caveLevel.Ores)
                    {
                        int oreSeed = _seed ^ (_blockDatabase.GetId(oreEntry.OreBlockName) * 73856093);

                        if (ShouldPlaceOre(worldX, worldY, oreSeed, oreEntry.Threshold, oreEntry.Scale))
                        {
                            ushort oreId = _blockDatabase.GetId(oreEntry.OreBlockName);
                            return (oreId, backgroundId);
                        }
                    }

                    return (stoneId, backgroundId);
                }
            }

            return (Block.AirId, Block.AirId);
        }
        private bool ShouldPlaceOre(
            int worldX,
            int worldY,
            int noiseSeed,
            float threshold,
            float scale)
        {
            float offsetX = Mathf.Sin(noiseSeed * 928.371f) * 1000f;
            float offsetY = Mathf.Cos(noiseSeed * 573.829f) * 1000f;
            return Mathf.PerlinNoise(worldX * scale + offsetX, worldY * scale + offsetY) > threshold;
        }

        public bool CanBreakBehindBlock(WorldPosition position)
        {
            var biome = _biomeProvider.GetBiome(position.x);
            int surfaceY = _surfaceHeightProvider.GetSurfaceY(position.x);
            return position.y > surfaceY - biome.Depth;
        }
    }
}