namespace World.Chunks.Generator.Providers
{
    public interface IBiomeProvider
    {
        public Biome GetBiome(int worldX, int seed);
    }
}