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
    [ContextMenu("Создать BlockInfo для всех спрайтов")]
        private void CreateBlockInfos()
        {
            // ищем все спрайты в проекте
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/World/Blocks/Textures" });
            int currentId = 0;

            foreach (string guid in guids)
            {
                currentId++;

                string path = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);

                // проверяем, есть ли уже такой блок
                if (blocks.Exists(b => b != null && b.Sprite == sprite))
                {
                    blocks[currentId].Id = (ushort)currentId;
                    EditorUtility.SetDirty(blocks[currentId]);
                    continue;
                }

                // создаём новый ScriptableObject
                BlockInfo block = ScriptableObject.CreateInstance<BlockInfo>();
                block.Name = sprite.name;
                block.Sprite = sprite;
                block.Id = (ushort)currentId;

                // сохраняем в папку ScriptableObjects
                string savePath = "Assets/World/Blocks/Objects/" + sprite.name + ".asset";
                AssetDatabase.CreateAsset(block, savePath);

                blocks.Add(block);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Создано {blocks.Count} BlockInfo");
        }
#endif
    }
}