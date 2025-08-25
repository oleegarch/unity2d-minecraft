using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace World.Items
{
    [CreateAssetMenu(menuName = "Items/New ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemInfo> items = new();

        private Dictionary<string, ushort> _nameToId;
        private Dictionary<ushort, ItemInfo> _byId;
        private Dictionary<ushort, ItemInfo> _byBlockId;

        private void OnEnable()
        {
            _byId = items.ToDictionary(b => b.Id);
            _byBlockId = items.ToDictionary(b => b.BlockId);
            _nameToId = items.ToDictionary(b => b.name, b => b.Id);
        }

        public ushort GetNextId() => (ushort)items.Count;

        public ItemInfo Get(ushort id) =>
            _byId[id];
            
        public ushort GetId(string name) =>
            _nameToId[name];

        public ItemInfo GetByBlockId(ushort blockId) =>
            _byBlockId[blockId];

        public bool TryGetByBlockId(ushort blockId, out ItemInfo itemInfo) =>
            _byBlockId.TryGetValue(blockId, out itemInfo);
    }
}