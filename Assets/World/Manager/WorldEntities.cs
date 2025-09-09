using UnityEngine;

namespace World.Chunks
{
    public class WorldEntities
    {
        private LayerMask _layerMask;

        public WorldEntities(LayerMask layerMask)
        {
            _layerMask = layerMask;
        }

        public bool HasEntityAtPoint(Vector3 position)
        {
            Collider2D collider = Physics2D.OverlapPoint(position, _layerMask);
            Debug.Log($"HasEntityAtPoint {collider}");
            return collider != null;
        }
        public bool HasEntityAtPoint(WorldPosition position)
        {
            return HasEntityAtPoint(position.ToVector3Int());
        }
    }
}