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
        [SerializeField] private WorldChunksCreator _worldChunksCreator;
        [SerializeField] private WorldChunksPreloader _worldChunksPreloader;
        [SerializeField] private WorldChunksVisible _visibility;
        [SerializeField] private EntityDatabase _database;
        [SerializeField] private Transform _spawnParent;
        [SerializeField] private Transform _localPlayerTransform;
        [SerializeField] private EntityInfo _localPlayerEntityInfo;

        private List<GameObject> _spawnedEntities = new();

        public void Enable()
        {
            _visibility.OnVisibleChunksChanged += HandleVisibleChanged;
            _worldChunksCreator.OnVisibleChunksLoaded += HandleChunksVisibleLoaded;
        }
        public void Disable()
        {
            _visibility.OnVisibleChunksChanged -= HandleVisibleChanged;
            _worldChunksCreator.OnVisibleChunksLoaded -= HandleChunksVisibleLoaded;
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
        private void HandleChunksVisibleLoaded()
        {
            int surfaceY = _worldManager.Generator.EntitiesSpawner.GetSurfaceY(0);
            float colliderHeight = _localPlayerEntityInfo.ColliderHeight;
            _localPlayerTransform.position = new Vector3(0f, surfaceY + colliderHeight, 0f);
        }

        public void SpawnEntity(GameObject entityPrefab, WorldPosition worldPosition)
        {
            Vector3 position = worldPosition.ToVector3Int();
            GameObject spawned = Instantiate(entityPrefab, position, Quaternion.identity, _spawnParent);
            EntityChunksPreloadWaiting waiting = spawned.GetComponent<EntityChunksPreloadWaiting>();
            _ = waiting.SetPreloader(_worldChunksPreloader).StartWait();
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