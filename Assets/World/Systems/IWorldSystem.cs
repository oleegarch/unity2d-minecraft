using World.Chunks;

namespace World.Systems
{
    public interface IWorldSystem
    {
        public void RegisterSystem(ChunksManager manager);
        public void UnregisterSystem(ChunksManager manager);
    }
}