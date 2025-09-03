using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace World.Items
{
    [CreateAssetMenu(menuName = "Items/New ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemInfo> items = new();

        private Dictionary<string, ItemInfo> _byName;
        private Dictionary<ushort, ItemInfo> _byId;
        private Dictionary<ushort, ItemInfo> _byBlockId;

        private void OnEnable()
        {
            _byId = items.ToDictionary(b => b.Id);
            _byBlockId = items.ToDictionary(b => b.BlockId);
            _byName = items.ToDictionary(b => b.name);
        }

        public ushort GetNextId() => (ushort)items.Count;

        public ItemInfo Get(ushort id) =>
            _byId[id];
        public ItemInfo GetByName(string name) =>
            _byName[name];
        public ItemInfo GetByBlockId(ushort blockId) =>
            _byBlockId[blockId];
            
        public ushort GetId(string name) =>
            _byName[name].Id;

        public bool TryGetByBlockId(ushort blockId, out ItemInfo itemInfo) =>
            _byBlockId.TryGetValue(blockId, out itemInfo);
    }
}