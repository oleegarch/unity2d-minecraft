using System.Collections.Generic;
using UnityEngine;
using World.Chunks;

namespace World.Entities
{
    public class WorldEntities : MonoBehaviour
    {
        [SerializeField] private LayerMask _entitiesLayerMask;
        [SerializeField] private WorldVisibleService _visibility;
        [SerializeField] private EntityDatabase _database;
        [SerializeField] private Transform _playerTransform;
        [SerializeField] private Transform _spawnParent;

        private List<GameObject> _spawnedKindMobs = new();

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
            Instantiate(_database.entities[0].Prefab, new Vector3(0f, 20f, 0f), Quaternion.identity, _spawnParent);
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