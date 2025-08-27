namespace World.Chunks.Generator.Providers
{
    public interface ISurfaceHeightProvider
    {
        public int GetSurfaceY(int worldX);
    }
}