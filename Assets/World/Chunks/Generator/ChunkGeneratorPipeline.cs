using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using World.Systems;
using World.Chunks.Generator.Steps;
using World.Chunks.Generator.Procedural;
using World.Chunks.Blocks;
using World.Rules;
using Cysharp.Threading.Tasks;

namespace World.Chunks.Generator
{
    public interface IChunkGenerator
    {
        public byte ChunkSize { get; }
        public WorldGlobalRules Rules { get; }
        public ChunkGeneratorConfig Config { get; }
        public IEntitiesSpawner EntitiesSpawner { get; }
        public void CacheComputation(RectInt rect);
        public void CacheComputation(HashSet<ChunkIndex> indexes);
        public void RegisterWorldSystems(WorldManager manager);
        public void UnregisterWorldSystems(WorldManager manager);
        public UniTask<Chunk> GenerateChunkAsync(ChunkIndex index);
    }

    // Composite generator orchestrates
    public class ChunkGeneratorPipeline : IChunkGenerator
    {
        public byte ChunkSize => _rules.ChunkSize;
        public WorldGlobalRules Rules => _rules;
        public ChunkGeneratorConfig Config => _config;
        public IEntitiesSpawner EntitiesSpawner => _entitiesSpawner;

        private readonly ChunkGeneratorConfig _config;
        private readonly WorldGlobalRules _rules;
        private readonly IEntitiesSpawner _entitiesSpawner;
        private readonly IChunkCreationStep _creationStep;
        private readonly IReadOnlyList<IChunkPostStep> _chunkPostProcessingSteps;
        private readonly IReadOnlyList<IChunkCacheStep> _chunkCachingSteps;
        private readonly IReadOnlyList<IWorldSystem> _worldSystems;

        public ChunkGeneratorPipeline(
        ChunkGeneratorConfig config,
        WorldGlobalRules rules,
        IEntitiesSpawner entitiesSpawner,
        IChunkCreationStep chunkCreationStep,
        IEnumerable<IChunkPostStep> chunkPostProcessingSteps,
        IEnumerable<IChunkCacheStep> chunkCachingSteps,
        IEnumerable<IWorldSystem> worldSystems)
        {
            _config = config;
            _rules = rules;
            _entitiesSpawner = entitiesSpawner;
            _creationStep = chunkCreationStep;
            _chunkPostProcessingSteps = chunkPostProcessingSteps.ToList();
            _chunkCachingSteps = chunkCachingSteps.ToList();
            _worldSystems = worldSystems.ToList();
        }

        public void CacheComputation(RectInt blocksVisibleRect)
        {
            foreach (var step in _chunkCachingSteps)
                step.CacheComputation(blocksVisibleRect);
        }
        public void CacheComputation(HashSet<ChunkIndex> indexes)
        {
            foreach (var step in _chunkCachingSteps)
                step.CacheComputation(indexes);
        }

        private Chunk GenerateChunk(ChunkIndex index)
        {
            // chunk creation step
            Chunk chunk = _creationStep.Execute(index, ChunkSize);

            // chunk post processing steps
            foreach (var step in _chunkPostProcessingSteps)
                step.Execute(chunk);

            return chunk;
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

        public async UniTask<Chunk> GenerateChunkAsync(ChunkIndex index)
        {
            return await UniTask.RunOnThreadPool(() => GenerateChunk(index));
        }
    }
}