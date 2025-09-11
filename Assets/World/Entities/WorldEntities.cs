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
        [SerializeField] private ChunksPreloader _worldChunksPreloader;
        [SerializeField] private ChunksVisible _visibility;
        [SerializeField] private EntityDatabase _database;
        [SerializeField] private Transform _spawnParent;
        [SerializeField] private Transform _localPlayerTransform;
        [SerializeField] private EntityInfo _localPlayerEntityInfo;
        [SerializeField] private EntityChunksDynamicPreloading _localPlayerChunksPreloading;

        private List<GameObject> _spawnedEntities = new();
        private List<GameObject> _currentActiveEntities = new();
        private List<EntityActivityToggler> _activatorsEntities = new();

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

        private void HandleVisibleChanged(RectInt chunksVisibleRect)
        {
            List<EntityWillSpawn> willSpawns = _worldManager.Generator.EntitiesSpawner.WhereToSpawnEntity(chunksVisibleRect);

            foreach (EntityWillSpawn willSpawn in willSpawns)
            {
                EntityInfo entityInfo = willSpawn.EntityInfo;
                WorldPosition worldPosition = willSpawn.SpawnAt;

                SpawnEntity(entityInfo.Prefab, worldPosition);
            }

            _currentActiveEntities.Clear();

            RectInt blocksVisibleRect = _visibility.BlocksVisibleRect;
            foreach (EntityActivityToggler activator in _activatorsEntities)
            {
                Vector3 entityPosition = activator.transform.position;
                Vector2Int entityPosition2Int = new Vector2Int(Mathf.FloorToInt(entityPosition.x), Mathf.FloorToInt(entityPosition.y));
                if (blocksVisibleRect.Contains(entityPosition2Int))
                {
                    activator.Enable();
                    _currentActiveEntities.Add(activator.gameObject);
                }
                else
                {
                    activator.Disable();
                    _currentActiveEntities.Remove(activator.gameObject);
                }
            }
        }
        private void HandleChunksVisibleLoaded()
        {
            int surfaceY = _worldManager.Generator.EntitiesSpawner.GetSurfaceY(0);
            float colliderHeight = _localPlayerEntityInfo.ColliderHeight;
            _localPlayerTransform.position = new Vector3(0f, surfaceY + colliderHeight, 0f);
            _localPlayerChunksPreloading.SetPreloader(_worldChunksPreloader);
        }

        public void SpawnEntity(GameObject entityPrefab, WorldPosition worldPosition)
        {
            Vector3 position = worldPosition.ToVector3Int();
            GameObject spawned = Instantiate(entityPrefab, position, Quaternion.identity, _spawnParent);

            if (spawned.TryGetComponent<EntityChunksPreloadWaiting>(out var waiting))
                _ = waiting.SetPreloader(_worldChunksPreloader).StartWait();

            if (spawned.TryGetComponent<EntityActivityToggler>(out var activator))
                _activatorsEntities.Add(activator);
                
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