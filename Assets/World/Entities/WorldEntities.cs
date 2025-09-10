using System.Collections.Generic;
using UnityEngine;
using World.Chunks;
using World.Chunks.Generator.Procedural;

namespace World.Entities
{
    public class WorldEntities : MonoBehaviour
    {
        [SerializeField] private LayerMask _entitiesLayerMask;
        [SerializeField] private WorldManager _worldManager;
        [SerializeField] private WorldVisibleService _visibility;
        [SerializeField] private EntityDatabase _database;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Transform _spawnParent;

        private List<GameObject> _spawnedEntities = new();

        private void OnEnable()
        {
            _visibility.OnVisibleChunksChanged += HandleVisibleChanged;
        }
        private void OnDisable()
        {
            _visibility.OnVisibleChunksChanged -= HandleVisibleChanged;
        }

        private void HandleVisibleChanged(RectInt viewRect)
        {
            List<EntityWillSpawn> willSpawns = _worldManager.Generator.EntitiesSpawner.WhereToSpawnEntity(viewRect);

            foreach (EntityWillSpawn willSpawn in willSpawns)
            {
                EntityInfo entityInfo = willSpawn.EntityInfo;
                WorldPosition worldPosition = willSpawn.SpawnAt;

                SpawnEntity(entityInfo.Prefab, worldPosition);
            }
        }

        public void SpawnEntity(GameObject entityPrefab, WorldPosition worldPosition)
        {
            Vector3 position = worldPosition.ToVector3Int();
            GameObject spawned = Instantiate(entityPrefab, position, Quaternion.identity, _spawnParent);
            _spawnedEntities.Add(spawned);
        }

        public bool HasEntityAtPoint(Vector3 position)
        {
            Collider2D collider = Physics2D.OverlapPoint(position, _entitiesLayerMask);
            return collider != null;
        }
        public bool HasEntityAtPoint(WorldPosition position)
        {
            return HasEntityAtPoint(position.ToVector3Int());
        }
    }
}