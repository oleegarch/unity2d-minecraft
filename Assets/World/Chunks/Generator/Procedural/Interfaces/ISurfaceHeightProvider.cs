namespace World.Chunks.Generator.Procedural
{
    public interface ISurfaceHeightProvider
    {
        public int GetSurfaceY(int worldX);
    }
}