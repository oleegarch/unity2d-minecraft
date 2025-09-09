using System;
using UnityEngine;

namespace World.Entities
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class EntityMovement : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _playerTransform;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 4.5f;
        [SerializeField] private float measureVelocityY = 0.1f;

        [Header("Ground Check Position Settings")]
        [SerializeField] private LayerMask groundCheckLayer;
        [SerializeField] private Transform groundCheckTransform;
        [SerializeField] private Vector2 groundCheckPositionSize;
        [SerializeField] private float groundCheckPositionAngle;

        private float _move;
        private bool _jumpRequest;

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

            // Запускаем физический прыжок, если запросили и стоим на земле
            if (_jumpRequest && IsGrounded)
            {
                _rigidbody.linearVelocityY = jumpForce;
            }

            // Обновляем параметры Animator
            _animator.SetBool("Running",   Running);
            _animator.SetBool("IsGrounded", IsGrounded);
            _animator.SetBool("IsJumping",  IsJumping);
            _animator.SetBool("IsFalling",  IsFalling);
        }
        private void FixedUpdate()
        {
            // Движение по горизонтали
            _rigidbody.linearVelocityX = _move * moveSpeed;

            // Поворот спрайта
            if (Running)
            {
                Vector3 scale = _playerTransform.localScale;
                scale.x = _move < 0f ? -1f : 1f;
                _playerTransform.localScale = scale;
            }
        }

        public void Move(float move)
        {
            _move = move;
            Running = move != 0;
        }
        public void Jump(bool requested)
        {
            _jumpRequest = requested;
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