namespace World.Chunks.Generator.Providers
{
    public interface IProceduralBlockProvider
    {
        public (ushort mainId, ushort backgroundId) GenerateBlock(int worldX, int worldY);
    }
}