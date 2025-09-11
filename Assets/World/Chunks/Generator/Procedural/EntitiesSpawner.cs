using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Entities;

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
    public class EntitySpawnProperties
    {
        [Tooltip("сущность которую будем спавнить")]
        public EntityInfo EntityInfo;
        
        [Tooltip("относительные координаты спавна сущности где каждая единица это блок (0x — это центр биома, 0y — это SurfaceY)")]
        public Vector2Int RelativeSpawnAt = Vector2Int.zero;
        
        [Tooltip("допустимая погрешность по горизонтали в координатах спавна каждой этой сущности")]
        public MinMaxInt OffsetRangeX = new MinMaxInt(-10, 10);

        [Tooltip("мин-макс количество этих сущностей для спавна")]
        public MinMaxInt Count = new MinMaxInt(3, 5);
    }

    public class EntitiesSpawner : IEntitiesSpawner
    {
        private readonly List<EntitiesSpawnerConfig> _entitiesSpawnerConfigs;
        private readonly IBiomeProvider _biomeProvider;
        private readonly ISurfaceYProvider _surfaceYProvider;
        private readonly int _seed;

        public EntitiesSpawner(List<EntitiesSpawnerConfig> configs, IBiomeProvider biomeProvider, ISurfaceYProvider surfaceYProvider, int seed)
        {
            _entitiesSpawnerConfigs = configs;
            _biomeProvider = biomeProvider;
            _surfaceYProvider = surfaceYProvider;
            _seed = seed;
        }

        public int GetSurfaceY(int worldX)
        {
            return _surfaceYProvider.GetSurfaceY(worldX);
        }
        public List<EntityWillSpawn> WhereToSpawnEntity(RectInt rect)
        {
            List<EntityWillSpawn> willSpawn = new();
            List<BiomeRange> biomeRanges = _biomeProvider.GetBiomeRanges(rect.xMin, rect.xMax);

            foreach (EntitiesSpawnerConfig config in _entitiesSpawnerConfigs)
            {
                BiomeRange biomeRange = biomeRanges.FirstOrDefault(br => br.Biome.Name == config.BiomeName);

                if (biomeRange == null) continue;

                foreach (EntitySpawnProperties properties in config.EntitiesProperties)
                {
                    var rand = new System.Random(_seed + properties.Count.Sum);
                    int randomCount = properties.Count.GetRandom(rand);

                    while (randomCount-- > 0)
                    {
                        int centerBiomeWorldX = biomeRange.Range.start + (biomeRange.Range.length / 2);
                        int offsetX = properties.OffsetRangeX.GetRandom(rand);
                        int relativeX = properties.RelativeSpawnAt.x;
                        int worldX = centerBiomeWorldX + offsetX + relativeX;

                        int relativeY = properties.RelativeSpawnAt.y;
                        int surfaceY = GetSurfaceY(worldX);
                        int colliderHeight = Mathf.CeilToInt(properties.EntityInfo.ColliderHeight);
                        int worldY = surfaceY + relativeY + colliderHeight;

                        WorldPosition spawnAt = new WorldPosition(worldX, worldY);

                        willSpawn.Add(new EntityWillSpawn
                        {
                            EntityInfo = properties.EntityInfo,
                            SpawnAt = spawnAt
                        });
                    }
                }
            }

            return willSpawn;
        }
    }
}