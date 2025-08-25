using UnityEngine;

namespace World.Items
{
    public class ItemDropped : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public void SetUp(ItemInfo info)
        {
            _spriteRenderer.sprite = info.Sprite;
        }
    }
}