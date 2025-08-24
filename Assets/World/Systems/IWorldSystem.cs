using System;
using World.Chunks;

namespace World.Systems
{
    public interface IWorldSystem : IDisposable
    {
        public void RegisterSystem(WorldManager manager);
    }
}