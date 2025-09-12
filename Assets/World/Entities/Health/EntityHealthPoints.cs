using System;
using UnityEngine;

namespace World.Entities
{
    public class EntityHealthPoints : MonoBehaviour
    {
        [Tooltip("Модификатор цвета спрайтов для анимации краснения")]
        [SerializeField] private EntitySpritesColorModifier _spritesColor;
        [Tooltip("Проверка нахождения на земле или в полёте")]
        [SerializeField] private EntityGroundCheck _groundCheck;

        [Tooltip("Максимальное количество здоровья")]
        [SerializeField] private float _maxHealth = 10f;

        [Header("Урон от падения")]
        [Tooltip("Минимальная высота для получения урона от падения")]
        [SerializeField] private float _minHeightForDamageFromFall = 4f;
        [Tooltip("Каждый блок высоты падения после минимального отнимает такое количество здоровья")]
        [SerializeField] private float _damageFromFallPerBlockHeight = 0.5f;
        private bool _fallingStarted = false;
        private float _fallingStartedAtY;

        private float _currentHealth;
        public float Health => _currentHealth;

        public event Action OnDied;

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }
        private void FixedUpdate()
        {
            if (_groundCheck.IsFalling && !_fallingStarted)
            {
                _fallingStarted = true;
                _fallingStartedAtY = transform.position.y;
            }
            else if (!_groundCheck.IsFalling && _fallingStarted)
            {
                float fallingHeight = _fallingStartedAtY - transform.position.y;
                DamageFromFall(fallingHeight);
                _fallingStarted = false;
            }
        }

        /// <summary>
        /// Урон наносимый другим существом
        /// </summary>
        /// <param name="fromPoint">Точка откуда был нанесён урон. Используется для отталкивания в противоположную сторону</param>
        /// <param name="damage">Сколько уменьшить количества здоровья</param>
        public void DamageFromEntity(Vector3 fromPoint, float damage)
        {

        }
        
        /// <summary>
        /// Урон от падения
        /// </summary>
        /// <param name="height">Высота падения в количестве блоков</param>
        public void DamageFromFall(float height)
        {
            if (height > _minHeightForDamageFromFall)
            {
                float heightDamage = height - _minHeightForDamageFromFall;
                float damage = _damageFromFallPerBlockHeight * heightDamage;
                _spritesColor.FadeAll(Color.red);
                Damage(damage);
            }
        }

        /// <summary>
        /// Урон при застревании в блоках
        /// </summary>
        public void DamageFromStuck()
        {

        }

        /// <summary>
        /// Уменьшить количество здоровья
        /// </summary>
        public void Damage(float damage)
        {
            _currentHealth -= damage;

            if (_currentHealth <= 0)
            {
                OnDied?.Invoke();
            }
        }
    }
}