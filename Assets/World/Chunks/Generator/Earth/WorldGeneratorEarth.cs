using UnityEngine;
using System.Collections.Generic;
using World.Chunks.Generator.Steps;
using World.Chunks.Generator.Procedural;
using World.Systems;
using World.Rules;

namespace World.Chunks.Generator
{    
    [CreateAssetMenu(menuName = "ChunkGenerators/EarthGeneratorConfig")]
    public class WorldGeneratorEarth : WorldGeneratorConfig
    {
        [Tooltip("размер биома")]
        public int BiomeWidth;

        [Tooltip("сколько блоков до следующего биомов будет идти смешивание SurfaceY")]
        public float SurfaceBlendWidth;

        [Tooltip("список биомов")]
        public List<Biome> Biomes;

        [Tooltip("какие растения генерировать в биомах")]
        public List<BiomePlants> Plants;

        [Tooltip("шахты по уровням")]
        public List<CaveLevel> CaveLevels;

        [Tooltip("сущности в биомах")]
        public List<EntitiesSpawnerConfig> EntitiesInBiomes;

        public override IWorldGenerator GetWorldGenerator(WorldEnvironment worldConfig, int seed)
        {
            // Global rules for this world
            var rules = new WorldGlobalRules();

            // Procedural providers
            var biomeProvider = new BiomeProvider(Biomes, _chunkSize, BiomeWidth, seed);
            var surfaceYProvider = new SurfaceYProvider(biomeProvider, _chunkSize, BiomeWidth, SurfaceBlendWidth, seed);

            // Procedural chunk generation step
            var blockGenerator = new BlockGenerator(worldConfig.BlockDatabase, biomeProvider, surfaceYProvider, CaveLevels, seed); // Procedural generation
            var creationStep = new ChunkCreationStep(blockGenerator);
            rules.SetCanBreakBehindBlock(blockGenerator.CanBreakBehindBlock);

            // Entities spawner
            var entitiesSpawner = new EntitiesSpawner(EntitiesInBiomes, biomeProvider, surfaceYProvider, seed);

            // Procedural post-processing steps
            var plants = new IPlantPlacer[] { new TreePlantPlacer(Plants, biomeProvider, surfaceYProvider, worldConfig.BlockDatabase, seed) }; // Plant placers
            var postSteps = new IChunkPostStep[] { new ChunkPlantsPostStep(plants) };

            // Cache computation for procedural generation steps
            var cacheSteps = new IChunkCacheStep[] { biomeProvider, surfaceYProvider };

            // Compose generator
            var worldSystems = new IWorldSystem[] { new BreakableByGravitySystem() };
            var composite = new WorldGeneratorPipeline(worldConfig, this, rules, entitiesSpawner, creationStep, postSteps, cacheSteps, worldSystems);

            return composite;
        }
    }
}