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

        private static Rect GetTightPixelRectForSprite(Sprite sprite)
        {
            float ppu = Mathf.Max(1f, sprite.pixelsPerUnit);

            Rect spritePixelRect = sprite.rect;
            Rect textureSpriteRect = sprite.textureRect;

            var tex = sprite.texture;
            Rect texRect = sprite.textureRect;

            int sx = Mathf.FloorToInt(texRect.x);
            int sy = Mathf.FloorToInt(texRect.y);
            int sw = Mathf.FloorToInt(texRect.width);
            int sh = Mathf.FloorToInt(texRect.height);

            Color32[] pixels;
            try
            {
                pixels = tex.GetPixels32();
            }
            catch
            {
                Debug.LogWarning($"Texture '{tex.name}' is not readable. Enable Read/Write in importer for accurate trimming. Falling back to full sprite rect.");
                return texRect;
            }

            int texW = tex.width;

            int left = sw, right = -1, bottom = sh, top = -1;
            const byte alphaThreshold = 1;

            for (int y = 0; y < sh; y++)
            {
                int ty = sy + y;
                int rowBase = ty * texW;
                for (int x = 0; x < sw; x++)
                {
                    int tx = sx + x;
                    Color32 c = pixels[rowBase + tx];
                    if (c.a > alphaThreshold)
                    {
                        if (x < left) left = x;
                        if (x > right) right = x;
                        if (y < bottom) bottom = y;
                        if (y > top) top = y;
                    }
                }
            }

            if (right < left || top < bottom)
            {
                return texRect;
            }

            int trimmedX = sx + left;
            int trimmedY = sy + bottom;
            int trimmedW = right - left + 1;
            int trimmedH = top - bottom + 1;

            Rect tightPixelRect = new Rect(trimmedX, trimmedY, trimmedW, trimmedH);

            float relativeTrimX = tightPixelRect.x - textureSpriteRect.x;
            float relativeTrimY = tightPixelRect.y - textureSpriteRect.y;

            float spriteFullUnitsW = spritePixelRect.width / ppu;
            float spriteFullUnitsH = spritePixelRect.height / ppu;

            float centerOffsetX = (1f - spriteFullUnitsW) * 0.5f;
            float centerOffsetY = (1f - spriteFullUnitsH) * 0.5f;

            float trimOffsetUnitsX = relativeTrimX / ppu;
            float trimOffsetUnitsY = relativeTrimY / ppu;

            float trimmedUnitsW = tightPixelRect.width / ppu;
            float trimmedUnitsH = tightPixelRect.height / ppu;

            return new Rect(
                centerOffsetX + trimOffsetUnitsX,
                centerOffsetY + trimOffsetUnitsY,
                trimmedUnitsW,
                trimmedUnitsH
            );
        }

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
                .Where(b => b != null && b.Sprite != null)
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

                // Собрать все текстуры блоков (оригинальные textures у спрайтов)
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
                var shader = Shader.Find("Custom/BlockTransparentRenderer");
                var material = new Material(shader)
                {
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

                for (int i = 0; i < infos.Count; i++)
                {
                    Sprite sprite = infos[i].Sprite;

                    blockAtlas.TextureUVs.Add(new BlockTextureUV
                    {
                        Id = infos[i].Id,
                        Rect = textureRects[i],
                        SpriteSizeUnits = GetTightPixelRectForSprite(sprite)
                    });
                }
                EditorUtility.SetDirty(blockAtlas);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✅ All atlases packed to " + OUTPUT_FOLDER);
        }
    }
}
#endif