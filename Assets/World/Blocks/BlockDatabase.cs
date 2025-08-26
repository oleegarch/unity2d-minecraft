using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using World.Items;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace World.Blocks
{
    [CreateAssetMenu(menuName = "Blocks/New BlockDatabase")]
    public class BlockDatabase : ScriptableObject
    {
        public List<BlockInfo> blocks = new();
        public ItemDatabase itemDatabase;

        private Dictionary<string, ushort> _nameToId;
        private Dictionary<ushort, BlockInfo> _byId;

        private void OnEnable()
        {
            _byId = blocks.ToDictionary(b => b.Id);
            _nameToId = blocks.ToDictionary(b => b.name, b => b.Id);
        }

        public ushort GetNextId() => (ushort)blocks.Count;

        public BlockInfo Get(ushort id) => _byId[id];
        public ushort GetId(string name) => _nameToId[name];

#if UNITY_EDITOR
        [ContextMenu("Создать BlockInfo для всех спрайтов")]
        private void CreateBlockInfos()
        {
            // ищем все спрайты в проекте
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/World/Blocks/Textures" });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                // проверяем, есть ли уже такой блок
                if (blocks.Exists(b => b != null && b.Sprite == sprite))
                    continue;

                // создаём новый ScriptableObject
                BlockInfo block = ScriptableObject.CreateInstance<BlockInfo>();
                block.Name = sprite.name;
                block.Sprite = sprite;
                block.Id = GetNextId();

                // сохраняем в папку ScriptableObjects
                string savePath = "Assets/World/Blocks/Objects/" + sprite.name + ".asset";
                AssetDatabase.CreateAsset(block, savePath);

                blocks.Add(block);
                EditorUtility.SetDirty(this);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Текущее количество блоков {blocks.Count} BlockInfo");
        }

        [ContextMenu("Создать ItemInfo для всех BlockInfo")]
        private void CreateItemInfos()
        {
            foreach (BlockInfo blockInfo in blocks)
            {
                if (!itemDatabase.TryGetByBlockId(blockInfo.Id, out ItemInfo item))
                {
                    // создаём новый ScriptableObject
                    item = ScriptableObject.CreateInstance<ItemInfo>();
                    item.Name = blockInfo.Name;
                    item.Sprite = blockInfo.Sprite;
                    item.Id = itemDatabase.GetNextId();
                    item.BlockId = blockInfo.Id;

                    // сохраняем в папку ScriptableObjects
                    string savePath = "Assets/World/Items/Objects/" + blockInfo.Name + ".asset";
                    AssetDatabase.CreateAsset(item, savePath);

                    itemDatabase.items.Add(item);
                    EditorUtility.SetDirty(itemDatabase);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}