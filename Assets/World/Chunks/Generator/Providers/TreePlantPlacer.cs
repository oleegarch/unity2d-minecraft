using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Blocks;
using World.Chunks.BlocksStorage;

namespace World.Chunks.Generator.Providers
{
    [Serializable]
    public class BiomePlants
    {
        [Tooltip("название биома где будут генерироваться данные растения")]
        public string BiomeName;

        [Tooltip("интервал между растениями")]
        public int Interval = 10;

        [Tooltip("допустимая погрешность в интервале между растениями")]
        public int OffsetRange = 2;

        [Tooltip("список возможных растений")]
        public List<BiomePlant> Plants;
    }
    [Serializable]
    public class BiomePlant
    {
        public enum PlantType
        {
            Tree,
            Cactus
        }

        [Tooltip("для Tree - название блока ствола, для Cactus название блока кактуса")]
        public string TrunkBlockName;

        [Tooltip("для Tree - название блока листвы")]
        public string LeafBlockName;

        [Tooltip("Tree - деревоподобная генерация, Cactus - просто генерация кактуса")]
        public PlantType Type;
        
        [Tooltip("для Tree - мин высота ствола, для Cactus мин высота кактуса")]
        public int MinHeight = 2;

        [Tooltip("для Tree - макс высота ствола, для Cactus макс высота кактуса")]
        public int MaxHeight = 6;
    }

    public class TreePlantPlacer : IPlantPlacer
    {
        private readonly List<BiomePlants> _biomePlantsList;
        private readonly IBiomeProvider _biomeProvider;
        private readonly ISurfaceHeightProvider _surfaceHeightProvider;
        private readonly BlockDatabase _blockDatabase;
        
        private Dictionary<string, BiomePlants> _biomeNameToBiomePlants;

        public TreePlantPlacer(
            List<BiomePlants> biomePlants,
            IBiomeProvider biomeProvider,
            ISurfaceHeightProvider surfaceHeightProvider,
            BlockDatabase blockDatabase)
        {
            _biomePlantsList = biomePlants;
            _biomeNameToBiomePlants = _biomePlantsList.ToDictionary(b => b.BiomeName);
            _biomeProvider = biomeProvider;
            _surfaceHeightProvider = surfaceHeightProvider;
            _blockDatabase = blockDatabase;
        }

        public void PlacePlants(Chunk chunk, int seed)
        {
            if (_biomePlantsList == null || _biomePlantsList.Count == 0)
                return;

            int chunkSize = chunk.Size;
            int chunkWorldStartX = chunk.Index.x * chunkSize;
            int overflowMargin = chunkSize / 2;

            for (int localX = -overflowMargin; localX < chunkSize + overflowMargin; localX++)
            {
                int worldX = chunkWorldStartX + localX;
                int surfaceY = _surfaceHeightProvider.GetSurfaceY(worldX, seed);
                
                Biome biome = _biomeProvider.GetBiome(worldX, seed);
                BiomePlants biomePlants = _biomeNameToBiomePlants[biome.Name];

                if (biomePlants == null || biomePlants.Plants == null || biomePlants.Plants.Count == 0)
                    continue;

                if (ShouldPlacePlant(biomePlants, worldX, seed, out BiomePlant selectedPlant))
                    GrowPlant(chunk, worldX, surfaceY, selectedPlant, seed);
            }
        }

        private bool ShouldPlacePlant(
            BiomePlants biomePlants,
            int worldX,
            int seed,
            out BiomePlant plant)
        {
            int bucketIndex = Mathf.FloorToInt((float)worldX / biomePlants.Interval);
            var random = new System.Random(seed ^ bucketIndex);
            int offset = random.Next(-biomePlants.OffsetRange, biomePlants.OffsetRange + 1);

            plant = biomePlants.Plants[random.Next(biomePlants.Plants.Count)];

            return bucketIndex * biomePlants.Interval + offset == worldX;
        }

        private void GrowPlant(
            Chunk chunk,
            int worldX,
            int surfaceY,
            BiomePlant plant,
            int seed)
        {
            var random = new System.Random(seed ^ worldX);
            int height = plant.MinHeight + random.Next(plant.MaxHeight - plant.MinHeight);
            int trunkBottomY = surfaceY + 1;
            int trunkTopY = trunkBottomY + height;

            // Build trunk
            Block trunkBlock = new Block(_blockDatabase.GetId(plant.TrunkBlockName));
            for (int y = trunkBottomY; y <= trunkTopY; y++)
            {
                if (chunk.TryGetBlockIndex(
                    new WorldPosition(worldX, y),
                    out BlockIndex blockIndex))
                {
                    chunk.Blocks.Set(blockIndex, trunkBlock, BlockLayer.Behind);
                    chunk.Render.OverrideBlockStyles(blockIndex, BlockStyles.BehindLikeMain, BlockLayer.Behind);
                }
            }

            // Build leaves if it's a tree type
            if (plant.Type == BiomePlant.PlantType.Tree)
            {
                Block leafBlock = new Block(_blockDatabase.GetId(plant.LeafBlockName));
                int halfHeight = height / 2;
                int innerRadius = height / 4;

                for (int deltaY = 0; deltaY <= halfHeight * 2; deltaY++)
                {
                    int rowRadius = halfHeight - deltaY / 2;
                    for (int deltaX = -rowRadius; deltaX <= rowRadius; deltaX++)
                    {
                        bool skipCenterLower = deltaX == 0 && deltaY <= innerRadius;
                        if (skipCenterLower)
                            continue;

                        int leafX = worldX + deltaX;
                        int leafY = trunkTopY - innerRadius + deltaY;

                        if (chunk.TryGetBlockIndex(
                            new WorldPosition(leafX, leafY),
                            out BlockIndex blockIndex))
                        {
                            chunk.Blocks.Set(blockIndex, leafBlock, BlockLayer.Behind);
                            chunk.Render.OverrideBlockStyles(blockIndex, BlockStyles.BehindLikeMain, BlockLayer.Behind);
                        }
                    }
                }
            }
        }
    }
}