using UnityEngine;
using System.Collections.Generic;
using World.Entities;

namespace World.Chunks.Generator.Procedural
{
    public class EntityWillSpawn
    {
        [Tooltip("сущность которую будем спавнить")]
        public EntityInfo EntityInfo;
        
        [Tooltip("мировые координаты спавна сущности")]
        public WorldPosition SpawnAt = WorldPosition.zero;
    }
    public interface IEntitiesSpawner
    {
        public int GetSurfaceY(int worldX);
        public List<EntityWillSpawn> WhereToSpawnEntity(RectInt rect);
    }
}