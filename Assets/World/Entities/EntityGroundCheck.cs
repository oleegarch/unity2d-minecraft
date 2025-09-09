using System;
using UnityEngine;

namespace World.Entities
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EntityGroundCheck : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Animator _animator;

        [Header("Movement Settings")]
        [SerializeField] private float measureVelocityY = 0.1f;

        [Header("Ground Check Position Settings")]
        [SerializeField] private LayerMask groundCheckLayer;
        [SerializeField] private Transform groundCheckTransform;
        [SerializeField] private Vector2 groundCheckPositionSize;
        [SerializeField] private float groundCheckPositionAngle;

        [NonSerialized] public bool Running;
        [NonSerialized] public bool IsGrounded;
        [NonSerialized] public bool IsJumping;
        [NonSerialized] public bool IsFalling;

        private void Update()
        {
            // Обновляем флаги по физике
            IsGrounded = Physics2D.OverlapBox((Vector2)groundCheckTransform.position, groundCheckPositionSize, groundCheckPositionAngle, groundCheckLayer);

            float velY = _rigidbody.linearVelocityY;
            IsJumping = velY >  measureVelocityY;
            IsFalling =  velY < -measureVelocityY;

            // Обновляем параметры Animator
            _animator.SetBool("IsGrounded", IsGrounded);
            _animator.SetBool("IsJumping", IsJumping);
            _animator.SetBool("IsFalling", IsFalling);
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheckTransform == null)
                return;

            Vector2 position = groundCheckTransform.position;

            // Проверка пересечения прямо в редакторе
            bool isGrounded = Physics2D.OverlapBox(position, groundCheckPositionSize, groundCheckPositionAngle, groundCheckLayer);

            // Цвет в зависимости от результата
            Gizmos.color = isGrounded ? Color.green : Color.red;

            // Применяем поворот и позицию для Gizmos
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(position, Quaternion.Euler(0, 0, groundCheckPositionAngle), Vector3.one);
            Gizmos.matrix = rotationMatrix;

            // Рисуем каркас бокса
            Gizmos.DrawWireCube(Vector3.zero, groundCheckPositionSize);

            // Сброс матрицы
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}