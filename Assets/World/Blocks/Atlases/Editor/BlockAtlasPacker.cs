#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.IO;

namespace World.Blocks.Atlases
{
    public class BlockAtlasPacker
    {
        // Путь для сохранения атласов
        private const string OBJECTS_FOLDER = "Assets/World/Blocks/Atlases/Objects";
        private const string OUTPUT_FOLDER = "Assets/World/Blocks/Atlases/Textures";
        private const string MATERIALS_FOLDER = "Assets/World/Blocks/Atlases/Materials";

        [MenuItem("Tools/Blocks/Pack All Atlases")]
        public static void PackAllAtlases()
        {
            // Убедимся, что папка вывода текстур существует
            if (!AssetDatabase.IsValidFolder(OUTPUT_FOLDER))
            {
                Debug.LogError($"Output folder not found at {OUTPUT_FOLDER}");
                return;
            }
            // Убедимся, что папка объектов атласов существует
            if (!AssetDatabase.IsValidFolder(OBJECTS_FOLDER))
            {
                Debug.LogError($"Atlas objects folder not found at {OBJECTS_FOLDER}");
                return;
            }
            // Убедимся, что папка материалов существует
            if (!AssetDatabase.IsValidFolder(MATERIALS_FOLDER))
            {
                Debug.LogError($"Materials folder not found at {MATERIALS_FOLDER}");
                return;
            }

            // Найти все BlockInfo
            string[] guids = AssetDatabase.FindAssets("t:BlockInfo");
            var allInfos = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<BlockInfo>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(b => b.Sprite != null)
                .ToList();

            // Группировать по категории
            var groups = allInfos.GroupBy(b => b.AtlasCategory);

            foreach (var group in groups)
            {
                var category = group.Key;
                var infos = group.ToList();

                // Определяем пути к ScriptableObject, материалу и атласу
                string assetPath = $"{OBJECTS_FOLDER}/{category}.asset";
                string materialPath = $"{MATERIALS_FOLDER}/{category}.mat";
                string atlasAssetPath = $"{OUTPUT_FOLDER}/{category}.asset";

                // Найти BlockAtlasInfo (ScriptableObject)
                BlockAtlasInfo blockAtlas = AssetDatabase.LoadAssetAtPath<BlockAtlasInfo>(assetPath);
                if (blockAtlas == null)
                {
                    // Атлас блоков должен быть создан вручную
                    Debug.LogError($"BlockAtlasInfo not found at {assetPath}");
                    continue;
                }

                // Собрать все текстуры блоков
                var textures = infos.Select(b => b.Sprite.texture).ToArray();

                // PackTextures (собираем все текстуры блоков в один атлас)
                Texture2D textureAtlas = new Texture2D(2048, 2048, TextureFormat.RGBA32, false);
                Rect[] textureRects = textureAtlas.PackTextures(textures, 8, 2048);
                
                textureAtlas.filterMode = FilterMode.Point;
                textureAtlas.wrapMode = TextureWrapMode.Clamp;

                // Сохраняем атлас как asset (текстура)
                textureAtlas.name = Path.GetFileNameWithoutExtension(atlasAssetPath);
                AssetUtils.OverwriteAsset(textureAtlas, atlasAssetPath);

                // Создать или обновить материал (материал для атласа)
                var reloadedTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasAssetPath);
                var shader = Shader.Find("Shader Graphs/ChunkUnlit");
                var material = new Material(shader) {
                    name = Path.GetFileNameWithoutExtension(materialPath),
                    mainTexture = reloadedTexture
                };

                // Перезаписать материал в файл
                AssetUtils.OverwriteAsset(material, materialPath);

                // Обновить BlockAtlasInfo (ScriptableObject)
                var reloadedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                blockAtlas.Category = category;
                blockAtlas.Texture = reloadedTexture;
                blockAtlas.Material = reloadedMaterial;
                blockAtlas.TextureUVs.Clear();
                blockAtlas.Epsilon = 1f / textureAtlas.width * 2f;
                for (int i = 0; i < infos.Count; i++)
                    blockAtlas.TextureUVs.Add(new BlockTextureUV { Id = infos[i].Id, Rect = textureRects[i] });
                EditorUtility.SetDirty(blockAtlas);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ All atlases packed to " + OUTPUT_FOLDER);
        }
    }
}
#endif