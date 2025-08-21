using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace World.Blocks.Atlases
{
    [Serializable]
    public struct BlockTextureUV
    {
        public ushort Id;
        public Rect Rect;
        public Rect SpriteSizeUnits;
    }

    public enum BlockAtlasCategory
    {
        Empty,
        MainTerrain,
        Ores,
        Surface
    }

    [CreateAssetMenu(menuName = "Blocks/New BlockAtlasInfo")]
    public class BlockAtlasInfo : ScriptableObject
    {
        public BlockAtlasCategory Category;
        public Texture2D Texture;
        public Material Material;
        public List<BlockTextureUV> TextureUVs;

        private Dictionary<ushort, Rect> _cachedUVsDict;
        public Rect GetRect(ushort id)
        {
            if (_cachedUVsDict == null)
                _cachedUVsDict = TextureUVs.ToDictionary(u => u.Id, u => u.Rect);

            return _cachedUVsDict[id];
        }

        private Dictionary<ushort, Rect> _cachedSpriteSizes;
        public Rect GetSpriteSize(ushort id)
        {
            if (_cachedSpriteSizes == null)
                _cachedSpriteSizes = TextureUVs.ToDictionary(u => u.Id, u => u.SpriteSizeUnits);

            return _cachedSpriteSizes[id];
        }
    }
}