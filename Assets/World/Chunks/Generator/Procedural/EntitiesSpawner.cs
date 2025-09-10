using System;
using System.Collections.Generic;
using UnityEngine;
using World.Chunks.BlocksStorage;

namespace World.Chunks.Generator.Procedural
{
    [Serializable]
    public class EntitiesSpawnerConfig
    {
        [Tooltip("название биома где будут спавниться сущности")]
        public string BiomeName;

        [Tooltip("список возможных сущностей с их настройками для спавна")]
        public List<EntitySpawnProperties> EntitiesProperties;
    }
    [Serializable]
    public class EntitySpawnProperties : EntityWillSpawn
    {
        [Tooltip("допустимая погрешность в координатах спавна каждой этой сущности")]
        public int OffsetRange = 10;

        [Tooltip("мин-макс количество этих сущностей для спавна")]
        public MinMaxInt MinMax = new MinMaxInt(3, 5);
    }

    public class EntitiesSpawner : IEntitiesSpawner
    {
        private readonly List<EntitiesSpawnerConfig> _entitiesSpawnerConfigs;
        private readonly IBiomeProvider _biomeProvider;
        private readonly int _seed;

        public EntitiesSpawner(List<EntitiesSpawnerConfig> configs, IBiomeProvider biomeProvider, int seed)
        {
            _entitiesSpawnerConfigs = configs;
            _biomeProvider = biomeProvider;
            _seed = seed;
        }

        public List<EntityWillSpawn> SpawnEntity(Chunk chunk)
        {
            List<EntityWillSpawn> willSpawn = new();

            // foreach (EntitiesSpawnerConfig config in _entitiesSpawnerConfigs)
            // {
            //     WorldPosition start = chunk.Index.ToWorldPosition(chunk.Size);
            //     for (int worldX = start.x; worldX < start.x + chunk.Size; worldX++)
            //     {
            //         Biome biome = _biomeProvider.GetBiome(worldX);

            //         if (biome.Name == config.BiomeName)
            //         {
                        
            //         }
            //     }
            // }

            return willSpawn;
        }
    }
}