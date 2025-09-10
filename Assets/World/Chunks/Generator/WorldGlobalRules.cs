using System;

namespace World.Rules
{
    public class WorldGlobalRules
    {
        // размер чанка в этом мире
        public readonly byte ChunkSize;

        // глобальное правило для определения: можем ли мы ломать блок позади на этих мировых координатам
        public Func<WorldPosition, bool> CanBreakBehindBlock;

        public WorldGlobalRules(byte chunkSize)
        {
            ChunkSize = chunkSize;
        }
    }
}