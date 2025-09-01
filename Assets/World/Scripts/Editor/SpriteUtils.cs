#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Утилиты для Sprite (Editor only).
/// GetTightPixelRectForSprite возвращает Rect в единицах спрайта (units),
/// отражающий обрезанный по прозрачным пикселям прямоугольник внутри исходного texture-ассета.
/// Работает с тем, что Sprite может быть упакован в SpriteAtlas — чтение выполняется из исходного ассета.
/// </summary>
public static class SpriteUtils
{
    /// <summary>
    /// Возвращает tight rect (в единицах спрайта) — область без прозрачных пикселей.
    /// Если чтение невозможно, возвращает «полный» rect (в единицах).
    /// </summary>
    public static Rect GetTightPixelRectForSprite(this Sprite sprite)
    {
        if (sprite == null) return new Rect(0, 0, 1, 1);

        float ppu = Mathf.Max(1f, sprite.pixelsPerUnit);

        // rect в исходном texture-ассете (в пикселях)
        Rect spritePixelRect = sprite.rect;

        // координаты и размеры внутри исходного texture
        int sx = Mathf.FloorToInt(spritePixelRect.x);
        int sy = Mathf.FloorToInt(spritePixelRect.y);
        int sw = Mathf.FloorToInt(spritePixelRect.width);
        int sh = Mathf.FloorToInt(spritePixelRect.height);

        // Значения по умолчанию — полное пространство спрайта (в units)
        float spriteFullUnitsW = spritePixelRect.width / ppu;
        float spriteFullUnitsH = spritePixelRect.height / ppu;
        float centerOffsetX = (1f - spriteFullUnitsW) * 0.5f;
        float centerOffsetY = (1f - spriteFullUnitsH) * 0.5f;

        // Попытаемся получить исходный texture-ассет (тот, что в проекте, а не runtime-атлас)
        string assetPath = AssetDatabase.GetAssetPath(sprite);
        Texture2D sourceTex = null;
        if (!string.IsNullOrEmpty(assetPath))
        {
            sourceTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }

        Color[] pixels = null;
        int texW = 0;

        // Функция fallback: вернуть полный rect в units
        Rect FallbackFullUnits() =>
            new Rect(centerOffsetX, centerOffsetY, spriteFullUnitsW, spriteFullUnitsH);

        if (sourceTex != null)
        {
            bool changedImporter = false;
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            bool originalReadable = false;

            try
            {
                if (importer != null)
                {
                    originalReadable = importer.isReadable;
                    if (!originalReadable)
                    {
                        // Временно включаем Read/Write, чтобы прочитать пиксели.
                        // Это вызывает reimport и может быть медленно при большом количестве текстур.
                        importer.isReadable = true;
                        importer.SaveAndReimport();
                        changedImporter = true;
                    }
                }

                // Теперь sourceTex должен быть читабельным
                if (!sourceTex.isReadable)
                {
                    Debug.LogWarning($"Texture asset '{assetPath}' is not readable even after toggling importer. Falling back to full rect.");
                    return FallbackFullUnits();
                }

                // читаем регион пикселей прямо из исходной текстуры
                // GetPixels(x,y,w,h) возвращает пиксели в порядке строки.
                pixels = sourceTex.GetPixels(sx, sy, sw, sh);
                texW = sw; // ширина выборки, используем локальную индексацию
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to read pixels from source texture '{assetPath}': {e.Message}. Falling back to full rect.");
                return FallbackFullUnits();
            }
            finally
            {
                // Восстанавливаем original readable флаг (если сменяли).
                if (changedImporter && importer != null)
                {
                    importer.isReadable = originalReadable;
                    importer.SaveAndReimport();
                }
            }
        }
        else
        {
            // Нету assetPath или не удалось загрузить — пробуем читать из runtime texture (если он доступен и читабелен)
            var runtimeTex = sprite.texture;
            if (runtimeTex != null)
            {
                Texture2D rt = runtimeTex as Texture2D;
                if (rt != null && rt.isReadable)
                {
                    // textureRect у спрайта — область в runtime-атласе. Используем её.
                    Rect texRect = sprite.textureRect;
                    int rx = Mathf.FloorToInt(texRect.x);
                    int ry = Mathf.FloorToInt(texRect.y);
                    int rw = Mathf.FloorToInt(texRect.width);
                    int rh = Mathf.FloorToInt(texRect.height);

                    try
                    {
                        pixels = rt.GetPixels(rx, ry, rw, rh);
                        texW = rw;
                    }
                    catch
                    {
                        Debug.LogWarning($"Runtime atlas texture for sprite '{sprite.name}' is not readable. Falling back to full rect.");
                        return FallbackFullUnits();
                    }
                }
                else
                {
                    Debug.LogWarning($"No readable source texture found for sprite '{sprite.name}'. Falling back to full rect.");
                    return FallbackFullUnits();
                }
            }
            else
            {
                Debug.LogWarning($"Sprite '{sprite.name}' has no texture. Falling back to full rect.");
                return FallbackFullUnits();
            }
        }

        if (pixels == null || pixels.Length == 0 || texW == 0)
            return FallbackFullUnits();

        // Ищем непустые пиксели в выборке (пиксели индексируются [y*texW + x])
        int left = sw, right = -1, bottom = sh, top = -1;
        const float alphaThreshold = 0.0039f; // ~1/255

        for (int y = 0; y < sh; y++)
        {
            int rowBase = y * texW;
            for (int x = 0; x < sw; x++)
            {
                Color c = pixels[rowBase + x];
                if (c.a > alphaThreshold)
                {
                    if (x < left) left = x;
                    if (x > right) right = x;
                    if (y < bottom) bottom = y;
                    if (y > top) top = y;
                }
            }
        }

        // Если ничего нет (полностью прозрачный) — вернуть полный rect
        if (right < left || top < bottom)
            return FallbackFullUnits();

        // tight rect в координатах исходного texture (пиксели, абсолютные)
        int trimmedX = sx + left;
        int trimmedY = sy + bottom;
        int trimmedW = right - left + 1;
        int trimmedH = top - bottom + 1;

        Rect tightPixelRect = new Rect(trimmedX, trimmedY, trimmedW, trimmedH);

        // относительный сдвиг внутри исходного sprite.rect (в пикселях)
        float relativeTrimX = tightPixelRect.x - spritePixelRect.x;
        float relativeTrimY = tightPixelRect.y - spritePixelRect.y;

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
}
#endif