namespace World.Chunks.Generator.Procedural
{
    public interface IBiomeProvider
    {
        public Biome GetBiome(int worldX);
    }
}