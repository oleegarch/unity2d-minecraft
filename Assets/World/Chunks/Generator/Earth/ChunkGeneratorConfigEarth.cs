using UnityEngine;
using System.Collections.Generic;
using World.Chunks.Generator.Steps;
using World.Chunks.Generator.Providers;
using World.Systems;
using World.Rules;

namespace World.Chunks.Generator
{    
    [CreateAssetMenu(menuName = "WorldGenerators/EarthGeneratorConfig")]
    public class ChunkGeneratorConfigEarth : ChunkGeneratorConfig
    {
        [Tooltip("размер биома")]
        public float BiomeWidth;

        [Tooltip("сколько блоков до следующего биомов будет идти смешивание SurfaceY")]
        public float SurfaceBlendWidth;

        [Tooltip("список биомов")]
        public List<Biome> Biomes;

        [Tooltip("какие растения генерировать в биомах")]
        public List<BiomePlants> Plants;

        [Tooltip("шахты по уровням")]
        public List<CaveLevel> CaveLevels;

        public override IChunkGenerator GetChunkGenerator(int seed)
        {
            // Create providers
            var biomeProvider = new BiomeProvider(Biomes, BiomeWidth, seed);
            var surfaceProvider = new SurfaceYProvider(biomeProvider, BiomeWidth, SurfaceBlendWidth, seed);

            // Create steps
            var blockGenerator = new ProceduralBlockGenerator(_blockDatabase, biomeProvider, surfaceProvider, CaveLevels, seed); // Procedural generation
            var creationStep = new ChunkCreationStep(blockGenerator);

            // global rules for this world
            var rules = new WorldGlobalRules()
            {
                CanBreakBehindBlock = blockGenerator.CanBreakBehindBlock
            };

            // Post processing steps
            var plants = new IPlantPlacer[] { new TreePlantPlacer(Plants, biomeProvider, surfaceProvider, _blockDatabase, seed) }; // Plant placers
            var postSteps = new IChunkPostStep[] { new ChunkPlantsPostStep(plants) };

            // Cache computation steps
            var cacheSteps = new IChunkCacheStep[] { biomeProvider, surfaceProvider };

            // Compose generator
            var settings = new ChunkGeneratorSettings(_chunkSize);
            var worldSystems = new IWorldSystem[] { new BreakableByGravitySystem() };
            var composite = new ChunkGeneratorPipeline(settings, rules, creationStep, postSteps, cacheSteps, worldSystems);

            // Inject into manager
            return composite;
        }
    }
}