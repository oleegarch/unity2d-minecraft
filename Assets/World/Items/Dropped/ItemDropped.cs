using System;
using System.Collections;
using UnityEngine;
using World.Inventories;

namespace World.Items
{
    public class ItemDropped : MonoBehaviour, IDisposable
    {
        [SerializeField] private Collider2D _collectCollider;
        [SerializeField] private float _throwForce = 15f;
        [SerializeField] private float _disalbeCollectDuration = 0.5f;

        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rb2d;

        [NonSerialized] public ItemStack Stack;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rb2d = GetComponent<Rigidbody2D>();

            if (_collectCollider != null)
                _collectCollider.enabled = false;
        }

        public void SetUp(ItemStack stack, ItemDatabase itemDatabase)
        {
            Stack = stack;

            // Меняем спрайт предмета
            _spriteRenderer.sprite = stack.Item.GetItemInfo(itemDatabase).Sprite;

            // Запускаем корутину включения коллайдера через задержку
            StartCoroutine(EnableColliderAfterDelay());
        }

        public void ThrowItem(Vector3 cursorPosition)
        {
            // Вектор направления от игрока до курсора
            Vector2 direction = (cursorPosition - transform.position).normalized;

            // Применяем силу к rigidbody предмета
            _rb2d.AddForce(direction * _throwForce, ForceMode2D.Impulse);
        }

        private IEnumerator EnableColliderAfterDelay()
        {
            yield return new WaitForSeconds(_disalbeCollectDuration);

            if (_collectCollider != null)
                _collectCollider.enabled = true;
        }

        public void Dispose()
        {
            Stack = null;
            Destroy(gameObject);
        }
    }
}