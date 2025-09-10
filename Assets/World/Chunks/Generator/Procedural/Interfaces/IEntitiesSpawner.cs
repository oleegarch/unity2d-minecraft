using UnityEngine;
using System.Collections.Generic;
using World.Entities;
using World.Chunks.BlocksStorage;

namespace World.Chunks.Generator.Procedural
{
    public class EntityWillSpawn
    {
        [Tooltip("сущность которую будем спавнить")]
        public EntityInfo EntityInfo;
        
        [Tooltip("координаты спавна сущности где каждая единица это блок (0x — это центр биома, 0y — это SurfaceY)")]
        public Vector2Int SpawnAt = Vector2Int.zero;
    }
    public interface IEntitiesSpawner
    {
        public List<EntityWillSpawn> SpawnEntity(Chunk chunk);
    }
}