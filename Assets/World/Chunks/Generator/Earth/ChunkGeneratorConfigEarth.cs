using UnityEngine;
using System.Collections.Generic;
using World.Chunks.Generator.Steps;
using World.Chunks.Generator.Procedural;
using World.Systems;
using World.Rules;

namespace World.Chunks.Generator
{    
    [CreateAssetMenu(menuName = "WorldGenerators/EarthGeneratorConfig")]
    public class ChunkGeneratorConfigEarth : ChunkGeneratorConfig
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

        public override IChunkGenerator GetChunkGenerator(int seed)
        {
            // Procedural providers
            var biomeProvider = new BiomeProvider(Biomes, BiomeWidth, seed);
            var surfaceYProvider = new SurfaceYProvider(biomeProvider, BiomeWidth, SurfaceBlendWidth, seed);

            // Procedural chunk generation step
            var blockGenerator = new BlockGenerator(_blockDatabase, biomeProvider, surfaceYProvider, CaveLevels, seed); // Procedural generation
            var creationStep = new ChunkCreationStep(blockGenerator);

            // Global rules for this world
            var rules = new WorldGlobalRules(chunkSize: 16)
            {
                CanBreakBehindBlock = blockGenerator.CanBreakBehindBlock
            };

            // Entities spawner
            var entitiesSpawner = new EntitiesSpawner(EntitiesInBiomes, biomeProvider, surfaceYProvider, seed);

            // Procedural post-processing steps
            var plants = new IPlantPlacer[] { new TreePlantPlacer(Plants, biomeProvider, surfaceYProvider, _blockDatabase, seed) }; // Plant placers
            var postSteps = new IChunkPostStep[] { new ChunkPlantsPostStep(plants) };

            // Cache computation for procedural generation steps
            var cacheSteps = new IChunkCacheStep[] { biomeProvider, surfaceYProvider };

            // Compose generator
            var worldSystems = new IWorldSystem[] { new BreakableByGravitySystem() };
            var composite = new ChunkGeneratorPipeline(this, rules, entitiesSpawner, creationStep, postSteps, cacheSteps, worldSystems);

            return composite;
        }
    }
}