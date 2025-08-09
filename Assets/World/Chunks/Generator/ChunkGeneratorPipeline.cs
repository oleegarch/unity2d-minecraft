using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using World.Chunks.Generator.Steps;
using World.Chunks.BlocksStorage;

using System.Diagnostics;

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

        private Stopwatch _stopwatch = new Stopwatch();

        public ChunkGeneratorPipeline(
            ChunkGeneratorSettings settings,
            IChunkCreationStep chunkCreationStep,
            IEnumerable<IChunkPostStep> chunkPostProcessingSteps)
        {
            ChunkSize = settings.ChunkSize;
            _creationStep = chunkCreationStep;
            _postProcessingSteps = chunkPostProcessingSteps.ToList();
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