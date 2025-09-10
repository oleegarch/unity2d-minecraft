using System;
using UnityEngine;

namespace World.Entities
{
    [RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(Animator))]
    public class EntityMovement : MonoBehaviour
    {
        [SerializeField] private EntityGroundCheck _groundCheck;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _playerTransform;

        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float jumpForce = 4.5f;

        private float _moveVelocity;
        private bool _jumpRequest;
        private float _moveSpeedMultiplier = 1f;

        public bool Running => _moveVelocity != 0;

        private void Update()
        {
            // Запускаем физический прыжок, если запросили и стоим на земле
            if (_jumpRequest && (_groundCheck == null || _groundCheck.IsGrounded))
            {
                _rigidbody.linearVelocityY = jumpForce;
            }

            // Обновляем параметры Animator
            _animator.SetBool("Running", Running);
            _animator.SetFloat("RunningSpeed", moveSpeed * _moveSpeedMultiplier);
        }
        private void FixedUpdate()
        {
            // Движение по горизонтали
            _rigidbody.linearVelocityX = _moveVelocity * moveSpeed * _moveSpeedMultiplier;

            // Поворот спрайта
            if (Running)
            {
                Vector3 scale = _playerTransform.localScale;
                scale.x = _moveVelocity < 0f ? -1f : 1f;
                _playerTransform.localScale = scale;
            }
        }

        public void Move(float move)
        {
            _moveVelocity = move;
        }
        public void MoveSpeed(float speed)
        {
            _moveSpeedMultiplier = speed;
        }
        public void Jump(bool requested)
        {
            _jumpRequest = requested;
        }
    }
}