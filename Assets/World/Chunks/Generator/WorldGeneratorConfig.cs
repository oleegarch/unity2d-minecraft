using UnityEngine;

namespace World.Chunks.Generator
{
    public abstract class WorldGeneratorConfig : ScriptableObject
    {
        [SerializeField] protected string _generatorName;
        [SerializeField] protected byte _chunkSize = 16;

        public string Name => _generatorName;
        public byte ChunkSize => _chunkSize;

        public abstract IWorldGenerator GetWorldGenerator(WorldEnvironment worldConfig, int seed);
    }
}