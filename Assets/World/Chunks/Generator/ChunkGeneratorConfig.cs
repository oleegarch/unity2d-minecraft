using UnityEngine;
using World.Blocks;

namespace World.Chunks.Generator
{
    public abstract class ChunkGeneratorConfig : ScriptableObject
    {
        public BlockDatabase BlockDatabase;
        public int ChunkSize = 16;
        public abstract IChunkGenerator GetChunkGenerator();
    }
}