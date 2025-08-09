using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using World.Chunks.Generator.Steps;
using World.Chunks.BlocksStorage;
using World.Chunks.Generator.Providers;

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
    }

    // Composite generator orchestrates
    public class ChunkGeneratorPipeline : IChunkGenerator
    {
        public static List<long> ProfilerChunksGenerationTicks = new();
        public static List<long> ProfilerChunksGenerationMS = new();

        public byte ChunkSize { get; }

        private readonly IChunkCreationStep _creationStep;
        private readonly IReadOnlyList<IChunkPostStep> _postProcessingSteps;
        private readonly IReadOnlyList<IChunkCacheStep> _chunkCachingSteps;

        public ChunkGeneratorPipeline(
            ChunkGeneratorSettings settings,
            IChunkCreationStep chunkCreationStep,
            IEnumerable<IChunkPostStep> chunkPostProcessingSteps,
            IEnumerable<IChunkCacheStep> chunkCachingSteps)
        {
            ChunkSize = settings.ChunkSize;
            _creationStep = chunkCreationStep;
            _postProcessingSteps = chunkPostProcessingSteps.ToList();
            _chunkCachingSteps = chunkCachingSteps.ToList();
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
    }
}