using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using World.Systems;
using World.Chunks.Generator.Steps;
using World.Chunks.BlocksStorage;

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
        public void CacheComputation(RectInt rect, int seed);
        public Task<Chunk> GenerateChunkAsync(ChunkIndex index, int seed);
        public void RegisterWorldSystems(WorldManager manager);
        public void UnregisterWorldSystems(WorldManager manager);
    }

    // Composite generator orchestrates
    public class ChunkGeneratorPipeline : IChunkGenerator
    {
        public byte ChunkSize { get; }

        private readonly IChunkCreationStep _creationStep;
        private readonly IReadOnlyList<IChunkPostStep> _postProcessingSteps;
        private readonly IReadOnlyList<IChunkCacheStep> _chunkCachingSteps;
        private readonly IReadOnlyList<IWorldSystem> _worldSystems;

        public ChunkGeneratorPipeline(
            ChunkGeneratorSettings settings,
            IChunkCreationStep chunkCreationStep,
            IEnumerable<IChunkPostStep> chunkPostProcessingSteps,
            IEnumerable<IChunkCacheStep> chunkCachingSteps,
            IEnumerable<IWorldSystem> worldSystems)
        {
            ChunkSize = settings.ChunkSize;
            _creationStep = chunkCreationStep;
            _postProcessingSteps = chunkPostProcessingSteps.ToList();
            _chunkCachingSteps = chunkCachingSteps.ToList();
            _chunkCachingSteps = chunkCachingSteps.ToList();
            _worldSystems = worldSystems.ToList();
        }

        public void CacheComputation(RectInt chunksVisibleRect, int seed)
        {
            RectInt blocksVisibleRect = new RectInt(
                chunksVisibleRect.xMin * ChunkSize,
                chunksVisibleRect.yMin * ChunkSize,
                chunksVisibleRect.width * ChunkSize,
                chunksVisibleRect.height * ChunkSize
            );

            foreach (var step in _chunkCachingSteps)
                step.CacheComputation(blocksVisibleRect, seed);
        }

        private Chunk GenerateChunk(ChunkIndex index, int seed)
        {
            // chunk creation step
            Chunk chunk = _creationStep.Execute(index, ChunkSize, seed);

            // chunk post processing steps
            foreach (var step in _postProcessingSteps)
                step.Execute(chunk, seed);

            return chunk;
        }

        public async Task<Chunk> GenerateChunkAsync(ChunkIndex index, int seed)
        {
            return await Task.Run(() => GenerateChunk(index, seed));
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