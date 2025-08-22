using UnityEngine;
using World.Blocks.Atlases;

namespace World.Blocks
{
    [CreateAssetMenu(menuName = "Blocks/New BlockInfo")]
    public class BlockInfo : ScriptableObject
    {
        public ushort Id;
        public string Name;
        public float Hardness = 1f;
        public Color OutlineColor = new Color(0f, 0f, 0f, 1f);
        public Sprite Sprite;
        public BlockAtlasCategory AtlasCategory;
        public BlockPlacementVariant[] AvailablePlacements =
        {
            BlockPlacementVariant.ForMain,
            BlockPlacementVariant.ForBehind
        };
        public bool HasTransparentPixels;
        public bool HasCustomCollider;
    }
}