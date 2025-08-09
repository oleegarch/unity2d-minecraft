using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using World.Chunks.Generator.Steps;
using World.Chunks.BlocksStorage;

using System.Diagnostics;
using UnityEngine;

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

        private Stopwatch _stopwatch = new Stopwatch();

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

        public void CacheComputation(RectInt rect, int seed)
        {
            rect.xMin = rect.xMin * ChunkSize;
            rect.xMax = rect.xMax * ChunkSize;
            rect.yMin = rect.yMin * ChunkSize;
            rect.yMax = rect.yMax * ChunkSize;

            UnityEngine.Debug.Log($"Chunk Pipeline rect in world position {rect}");

            foreach (var step in _chunkCachingSteps)
                step.CacheComputation(rect, seed);
        }

        private Chunk GenerateChunk(ChunkIndex index, int seed)
        {
            _stopwatch.Restart();
            
            // chunk creation step
            Chunk chunk = _creationStep.Execute(index, ChunkSize, seed);

            // chunk post processing steps
            foreach (var step in _postProcessingSteps)
                step.Execute(chunk, seed);

            _stopwatch.Stop();

            ProfilerChunksGenerationTicks.Add(_stopwatch.ElapsedTicks);
            ProfilerChunksGenerationMS.Add(_stopwatch.ElapsedMilliseconds);

            return chunk;
        }

        public async Task<Chunk> GenerateChunkAsync(ChunkIndex index, int seed)
        {
            return await Task.Run(() => GenerateChunk(index, seed));
        }
    }
}