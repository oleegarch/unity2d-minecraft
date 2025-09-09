using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace World.Entities
{
    [CreateAssetMenu(menuName = "Entity/New EntityDatabase")]
    public class EntityDatabase : ScriptableObject
    {
        public List<EntityInfo> entities = new();

        private Dictionary<string, EntityInfo> _byName;
        private Dictionary<ushort, EntityInfo> _byId;

        private void OnEnable()
        {
            _byId = entities.ToDictionary(b => b.Id);
            _byName = entities.ToDictionary(b => b.name);
        }

        public ushort GetNextId() => (ushort)entities.Count;

        public EntityInfo Get(ushort id) =>
            _byId[id];
        public EntityInfo Get(string name) =>
            _byName[name];
            
        public ushort GetId(string name) =>
            _byName[name].Id;
    }
}