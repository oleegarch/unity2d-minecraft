using System;
using UnityEngine;

namespace World.Items
{
    public class ItemDropped : MonoBehaviour, IDisposable
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public ItemInfo ItemInfo;

        public void SetUp(ItemInfo info)
        {
            ItemInfo = info;
            _spriteRenderer.sprite = info.Sprite;
        }

        public void Dispose()
        {
            ItemInfo = null;
            Destroy(gameObject);
        }
    }
}