using UnityEngine;
using System.Collections.Generic;
using World.Chunks.Generator.Steps;
using World.Chunks.Generator.Providers;

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

        public override IChunkGenerator GetChunkGenerator()
        {
            // Create providers
            var biomeProvider = new BiomeProvider(Biomes, BiomeWidth);
            var surfaceProvider = new PerlinSurfaceHeightProvider(biomeProvider, BiomeWidth, SurfaceBlendWidth);

            // Create steps
            var blockGenerator = new ProceduralEarthBlockGenerator(BlockDatabase, biomeProvider, surfaceProvider, CaveLevels); // Procedural Earth generation
            var creationStep = new ChunkProceduralCreation(blockGenerator);

            // Post processing steps
            var plants = new IPlantPlacer[] { new TreePlantPlacer(Plants, biomeProvider, surfaceProvider, BlockDatabase) }; // Plant placers
            var postSteps = new IChunkPostStep[] { new PlantPlaceStep(plants) };

            // Compose generator
            var settings = new ChunkGeneratorSettings(chunkSize: 16);
            var composite = new ChunkGeneratorPipeline(settings, creationStep, postSteps);

            // Inject into manager
            return composite;
        }
    }
}