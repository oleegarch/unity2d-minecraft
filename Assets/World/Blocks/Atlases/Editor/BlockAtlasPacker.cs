#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace World.Blocks.Atlases
{
    public class BlockAtlasPacker
    {
        private const string OBJECTS_FOLDER = "Assets/World/Blocks/Atlases/Objects";
        private const string OUTPUT_FOLDER = "Assets/World/Blocks/Atlases/Textures";
        private const string MATERIALS_FOLDER = "Assets/World/Blocks/Atlases/Materials";

        [MenuItem("Tools/Blocks/Pack All Atlases")]
        public static void PackAllAtlases()
        {
            // Проверки папок
            if (!AssetDatabase.IsValidFolder(OUTPUT_FOLDER))
            {
                Debug.LogError($"Output folder not found at {OUTPUT_FOLDER}");
                return;
            }
            if (!AssetDatabase.IsValidFolder(OBJECTS_FOLDER))
            {
                Debug.LogError($"Atlas objects folder not found at {OBJECTS_FOLDER}");
                return;
            }
            if (!AssetDatabase.IsValidFolder(MATERIALS_FOLDER))
            {
                Debug.LogError($"Materials folder not found at {MATERIALS_FOLDER}");
                return;
            }

            // Удаляем только генерируемые ассеты
            RemoveGeneratedAssets();

            string[] guids = AssetDatabase.FindAssets("t:BlockInfo");
            var allInfos = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<BlockInfo>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(b => b != null && b.Sprite != null)
                .ToList();

            var groups = allInfos.GroupBy(b => b.AtlasCategory);

            foreach (var group in groups)
            {
                var category = group.Key;
                var infos = group.ToList();

                string assetPath = $"{OBJECTS_FOLDER}/{category}.asset";
                string atlasTexturePath = $"{OUTPUT_FOLDER}/{category}.asset";

                BlockAtlasInfo blockAtlas = AssetDatabase.LoadAssetAtPath<BlockAtlasInfo>(assetPath);
                if (blockAtlas == null)
                {
                    Debug.LogError($"BlockAtlasInfo not found at {assetPath}");
                    continue;
                }

                // Обновляем BlockAtlasInfo
                blockAtlas.TextureUVs.Clear();
                blockAtlas.MaterialTemplate = GetMaterialTemplateByAtlas(blockAtlas);

                // Генерируем атлас текстур
                Texture2D atlasTexture = PackSpritesToTexture(infos, blockAtlas);
                atlasTexture.name = category;
                AssetDatabase.CreateAsset(atlasTexture, atlasTexturePath);

                // Помечаем всё изменённым для сохранения
                foreach (var info in infos) EditorUtility.SetDirty(info);
                EditorUtility.SetDirty(blockAtlas);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ All Block Atlases generated as Texture2D");
        }

        private static Texture2D PackSpritesToTexture(List<BlockInfo> infos, BlockAtlasInfo atlas)
        {
            // Собрать все текстуры блоков (оригинальные textures у спрайтов)
            var textures = infos.Select(b => b.Sprite.texture).ToArray();

            // PackTextures (собираем все текстуры блоков в один атлас)
            Texture2D textureAtlas = new Texture2D(2048, 2048, TextureFormat.RGBA32, false);
            Rect[] textureRects = textureAtlas.PackTextures(textures, 8, 2048);

            for (int i = 0; i < infos.Count; i++)
            {
                BlockInfo info = infos[i];
                info.VisibleSpriteRect = info.Sprite.GetTightPixelRectForSprite();
                atlas.TextureUVs.Add(new BlockTextureUV
                {
                    Id = info.Id,
                    Rect = textureRects[i]
                });
            }

            textureAtlas.filterMode = FilterMode.Point;
            textureAtlas.wrapMode = TextureWrapMode.Clamp;

            atlas.Texture = textureAtlas;

            return textureAtlas;
        }

        private static Material GetMaterialTemplateByAtlas(BlockAtlasInfo atlas)
        {
            string materialName = $"Block{atlas.RenderMode}";
            string materialPath = $"{MATERIALS_FOLDER}/{materialName}.mat";
            return AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        }

        private static void RemoveGeneratedAssets()
        {
            if (!AssetDatabase.IsValidFolder(OUTPUT_FOLDER))
                return;

            // Получаем все файлы и папки внутри OUTPUT_FOLDER
            string[] guids = AssetDatabase.FindAssets("", new[] { OUTPUT_FOLDER });
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(path))
                {
                    AssetDatabase.DeleteAsset(path);
                    Debug.Log($"Deleted: {path}");
                }
            }
        }
    }
}
#endif