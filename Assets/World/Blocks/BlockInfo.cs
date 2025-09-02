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
        public Rect VisibleSpriteRect;
        public BlockAtlasCategory AtlasCategory;
        public BlockPlacementVariant[] AvailablePlacements =
        {
            BlockPlacementVariant.ForMain,
            BlockPlacementVariant.ForBehind
        };

        [Tooltip("Имеет ли спрайт пиксели с прозрачностью")]
        public bool HasTransparentPixels;

        [Tooltip("Если коллайдер не ровный квадрат 1x1 — нужно поставить флаг true")]
        public bool HasCustomCollider;

        [Tooltip("Ломается ли блок если у него нет опоры")]
        public bool BreakableByGravity;

        [Tooltip("Имеется ли у этого блока инвентарь (для сундуков)")]
        public int InventorySlotCount;

        [Tooltip("Можно ли открыть верстак по этому блоку")]
        public bool HasCraftingInventory;
    }
}