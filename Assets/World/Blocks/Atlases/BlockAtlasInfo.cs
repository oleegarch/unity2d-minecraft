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

    [CreateAssetMenu(menuName = "Blocks/New BlockAtlasInfo")]
    public class BlockAtlasInfo : ScriptableObject
    {
        public BlockAtlasCategory Category;
        public Material MaterialTemplate;
        public List<BlockTextureUV> TextureUVs;
        public bool IsTransparent;

        private Dictionary<ushort, Rect> _cachedUVsDict;
        public Rect GetRect(ushort id)
        {
            if (_cachedUVsDict == null)
                _cachedUVsDict = TextureUVs.ToDictionary(u => u.Id, u => u.Rect);

            return _cachedUVsDict[id];
        }

        private Material _cachedMaterial;
        public Material GetMaterial(BlockDatabase blockDatabase)
        {
            if (_cachedMaterial != null) return _cachedMaterial;

            var firstBlockId = TextureUVs[0].Id; // любой BlockId
            var firstBlock = blockDatabase.Get(firstBlockId);
            var texture = firstBlock.Sprite.texture;
            _cachedMaterial = new Material(MaterialTemplate);
            _cachedMaterial.mainTexture = texture;
            return _cachedMaterial;
        }
    }
}