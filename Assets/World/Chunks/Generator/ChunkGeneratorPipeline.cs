using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using World.Systems;
using World.Chunks.Generator.Steps;
using World.Chunks.BlocksStorage;
using World.Rules;

namespace World.Chunks.Generator
{
    public class ChunkGeneratorSettings
    {
        public readonly byte ChunkSize;

        public ChunkGeneratorSettings(byte chunkSize)
        {
            ChunkSize = chunkSize;
        }
    }
    public interface IChunkGenerator
    {
        public byte ChunkSize { get; }
        public WorldGlobalRules Rules { get; }
        public ChunkGeneratorConfig Config { get; }
        public void CacheComputation(RectInt rect);
        public Task<Chunk> GenerateChunkAsync(ChunkIndex index);
        public void RegisterWorldSystems(WorldManager manager);
        public void UnregisterWorldSystems(WorldManager manager);
    }

    // Composite generator orchestrates
    public class ChunkGeneratorPipeline : IChunkGenerator
    {
        public byte ChunkSize => _settings.ChunkSize;
        public WorldGlobalRules Rules => _rules;
        public ChunkGeneratorConfig Config => _config;

        private readonly ChunkGeneratorConfig _config;
        private readonly ChunkGeneratorSettings _settings;
        private readonly WorldGlobalRules _rules;
        private readonly IChunkCreationStep _creationStep;
        private readonly IReadOnlyList<IChunkPostStep> _postProcessingSteps;
        private readonly IReadOnlyList<IChunkCacheStep> _chunkCachingSteps;
        private readonly IReadOnlyList<IWorldSystem> _worldSystems;

        public ChunkGeneratorPipeline(
            ChunkGeneratorConfig config,
            ChunkGeneratorSettings settings,
            WorldGlobalRules rules,
            IChunkCreationStep chunkCreationStep,
            IEnumerable<IChunkPostStep> chunkPostProcessingSteps,
            IEnumerable<IChunkCacheStep> chunkCachingSteps,
            IEnumerable<IWorldSystem> worldSystems)
        {
            _config = config;
            _settings = settings;
            _rules = rules;
            _creationStep = chunkCreationStep;
            _postProcessingSteps = chunkPostProcessingSteps.ToList();
            _chunkCachingSteps = chunkCachingSteps.ToList();
            _chunkCachingSteps = chunkCachingSteps.ToList();
            _worldSystems = worldSystems.ToList();
        }

        public void CacheComputation(RectInt chunksVisibleRect)
        {
            RectInt blocksVisibleRect = new RectInt(
                chunksVisibleRect.xMin * ChunkSize,
                chunksVisibleRect.yMin * ChunkSize,
                chunksVisibleRect.width * ChunkSize,
                chunksVisibleRect.height * ChunkSize
            );

            foreach (var step in _chunkCachingSteps)
                step.CacheComputation(blocksVisibleRect);
        }

        private Chunk GenerateChunk(ChunkIndex index)
        {
            // chunk creation step
            Chunk chunk = _creationStep.Execute(index, ChunkSize);

            // chunk post processing steps
            foreach (var step in _postProcessingSteps)
                step.Execute(chunk);

            return chunk;
        }

        public async Task<Chunk> GenerateChunkAsync(ChunkIndex index)
        {
            return await Task.Run(() => GenerateChunk(index));
        }

        public void RegisterWorldSystems(WorldManager manager)
        {
            foreach (var ws in _worldSystems)
                ws.RegisterSystem(manager);
        }
        public void UnregisterWorldSystems(WorldManager manager)
        {
            foreach (var ws in _worldSystems)
                ws.Dispose();
        }
    }
}