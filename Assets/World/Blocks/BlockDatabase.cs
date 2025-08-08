using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace World.Blocks
{
    [CreateAssetMenu(menuName = "Blocks/New BlockDatabase")]
    public class BlockDatabase : ScriptableObject
    {
        public List<BlockInfo> blocks;

        private Dictionary<string, ushort> _nameToId;
        private Dictionary<ushort, BlockInfo> _byId;

        private void OnEnable()
        {
            _byId = blocks.ToDictionary(b => b.Id);
            _nameToId = blocks.ToDictionary(b => b.name, b => b.Id);
        }

        public BlockInfo Get(ushort id) => _byId[id];
        public ushort GetId(string name) => _nameToId[name];

#if UNITY_EDITOR
        [ContextMenu("Установить Id блоков последовательно их сортировке в списке")]
        private void SetBlocksIdSortedInList()
        {
            ushort index = 0;
            foreach (BlockInfo info in blocks)
            {
                info.Id = index++;
                EditorUtility.SetDirty(info);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [ContextMenu("Установить во всех блоках название спрайта в качестве Name поле блока")]
        private void SetNameOfTextureName()
        {
            foreach (BlockInfo info in blocks)
            {
                if (info.Sprite != null)
                {
                    info.Name = info.Sprite.name;
                    EditorUtility.SetDirty(info);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}