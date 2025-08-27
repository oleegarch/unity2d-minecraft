namespace World.Chunks.Generator.Procedural
{
    public interface IProceduralBlockProvider
    {
        public (ushort mainId, ushort backgroundId) GenerateBlock(int worldX, int worldY);
    }
}