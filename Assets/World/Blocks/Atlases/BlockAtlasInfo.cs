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
    }

    [Serializable]
    public enum RenderMode
    {
        Opaque,
        Cutout,
        Transparent
    }

    [CreateAssetMenu(menuName = "Blocks/New BlockAtlasInfo")]
    public class BlockAtlasInfo : ScriptableObject
    {
        public BlockAtlasCategory Category;
        public Material MaterialTemplate;
        public RenderMode RenderMode;
        public Texture2D Texture;
        public List<BlockTextureUV> TextureUVs;

        private Dictionary<ushort, Rect> _cachedUVsDict;
        public Rect GetRect(ushort id)
        {
            if (_cachedUVsDict == null)
                _cachedUVsDict = TextureUVs.ToDictionary(u => u.Id, u => u.Rect);

            return _cachedUVsDict[id];
        }

        private Material _cachedMaterial;
        public Material GetMaterial()
        {
            if (_cachedMaterial != null) return _cachedMaterial;
            _cachedMaterial = new Material(MaterialTemplate);
            _cachedMaterial.mainTexture = Texture;
            return _cachedMaterial;
        }
    }
}