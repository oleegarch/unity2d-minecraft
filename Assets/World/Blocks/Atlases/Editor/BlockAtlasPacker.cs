#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using System.Linq;
using System.IO;

namespace World.Blocks.Atlases
{
    public class BlockAtlasPacker
    {
        // Путь для сохранения атласов
        private const string OBJECTS_FOLDER = "Assets/World/Blocks/Atlases/Objects";
        private const string OUTPUT_FOLDER = "Assets/World/Blocks/Atlases/Textures"; // теперь сюда будем сохранять .spriteatlas
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

            // Найти все BlockInfo
            string[] guids = AssetDatabase.FindAssets("t:BlockInfo");
            var allInfos = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<BlockInfo>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(b => b != null && b.Sprite != null)
                .ToList();

            // Группировать по категории
            var groups = allInfos.GroupBy(b => b.AtlasCategory);

            foreach (var group in groups)
            {
                var category = group.Key;
                var infos = group.ToList();

                // Путь к ScriptableObject и материалу и атласу
                string assetPath = $"{OBJECTS_FOLDER}/{category}.asset";
                string atlasAssetPath = $"{OUTPUT_FOLDER}/{category}.spriteatlas"; // создаём SpriteAtlas

                // Найти BlockAtlasInfo (ScriptableObject)
                BlockAtlasInfo blockAtlas = AssetDatabase.LoadAssetAtPath<BlockAtlasInfo>(assetPath);
                if (blockAtlas == null)
                {
                    Debug.LogError($"BlockAtlasInfo not found at {assetPath}");
                    continue;
                }

                // Удаляем только генерируемые ассеты для этой категории (без трогания .asset в Objects)
                RemoveGeneratedAssetsForCategory(category);

                // Создаём SpriteAtlas и добавляем туда все спрайты из группы
                var atlas = new SpriteAtlas();

                // Настройки упаковки (можно подправить при необходимости)
                var packingSettings = new SpriteAtlasPackingSettings
                {
                    blockOffset = 1,
                    padding = 2,
                    enableRotation = false,
                    enableTightPacking = false
                };
                atlas.SetPackingSettings(packingSettings);

                // Текстурные настройки
                var texSettings = new SpriteAtlasTextureSettings
                {
                    readable = false,
                    generateMipMaps = false,
                    sRGB = true
                };
                atlas.SetTextureSettings(texSettings);

                // Добавляем все спрайты как packables
                UnityEngine.Object[] packables = infos.Select(i => (UnityEngine.Object)i.Sprite).ToArray();
                atlas.Add(packables);

                // Сохраняем SpriteAtlas как asset (перезапись возможна, потому удаляли файл раньше)
                AssetDatabase.CreateAsset(atlas, atlasAssetPath);

                // Сохраняем/применяем изменения и гарантируем импорт
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(atlasAssetPath);
                AssetDatabase.Refresh();

                // Принудительно упакуем атласы для текущей платформы, чтобы получить итоговую текстуру
                SpriteAtlasUtility.PackAtlases(new[] { atlas }, EditorUserBuildSettings.activeBuildTarget);

                // После упаковки — убедимся, что ассеты импортированы и доступны
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // --- Обновляем BlockAtlasInfo ---
                blockAtlas.Category = category;
                blockAtlas.MaterialTemplate = GetMaterialTemplateByAtlas(blockAtlas);
                blockAtlas.TextureUVs.Clear();

                // Пройдёмся по каждому спрайту и запишем нормализованные UV относительно texture, в которой лежит этот спрайт
                for (int i = 0; i < infos.Count; i++)
                {
                    BlockInfo info = infos[i];
                    Sprite sprite = info.Sprite;

                    info.VisibleSpriteRect = sprite.GetTightPixelRectForSprite();
                    EditorUtility.SetDirty(info);
                    
                    Texture2D spriteTex = sprite.texture;
                    if (spriteTex == null)
                    {
                        Debug.LogError($"Sprite {sprite.name} has no texture after packing. Make sure pack succeeded for category {category}.");
                        continue;
                    }

                    Rect pixelRect = sprite.textureRect; // пиксели внутри своей texture

                    Rect normalized = new Rect(
                        pixelRect.x / (float)spriteTex.width,
                        pixelRect.y / (float)spriteTex.height,
                        pixelRect.width / (float)spriteTex.width,
                        pixelRect.height / (float)spriteTex.height
                    );

                    blockAtlas.TextureUVs.Add(new BlockTextureUV
                    {
                        Id = infos[i].Id,
                        Rect = normalized
                    });
                }

                EditorUtility.SetDirty(blockAtlas);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ All SpriteAtlases packed to " + OUTPUT_FOLDER);
        }

        private static Material GetMaterialTemplateByAtlas(BlockAtlasInfo atlas)
        {
            string materialName = $"Block{atlas.RenderMode.ToString()}";
            string materialPath = $"{MATERIALS_FOLDER}/{materialName}.mat";
            return AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        }

        private static void RemoveGeneratedAssetsForCategory(string category)
        {
            string atlasPath = $"{OUTPUT_FOLDER}/{category}.spriteatlas";

            if (AssetDatabase.LoadAssetAtPath<Object>(atlasPath) != null)
            {
                AssetDatabase.DeleteAsset(atlasPath);
                Debug.Log($"Deleted old atlas: {atlasPath}");
            }
        }
    }
}
#endif