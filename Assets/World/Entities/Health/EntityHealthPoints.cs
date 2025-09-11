using System;
using UnityEngine;

namespace World.Entities
{
    public class EntityHealthPoints : MonoBehaviour
    {
        [Tooltip("Максимальное количество здоровья")]
        [SerializeField] private float _maxHealth;

        private float _currentHealth;
        public float Health => _currentHealth;

        public event Action OnDied;

        private void Awake()
        {
            _currentHealth = _maxHealth;
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