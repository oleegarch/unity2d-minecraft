using System;
using UnityEngine;
using UnityEngine.InputSystem;
using World.InputActions;

namespace World.Entities.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private WorldInputManager _inputManager;
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Transform _groundCheck;
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

        private float _moveInput;
        private bool _jumpRequest;

        [NonSerialized] public bool Running;
        [NonSerialized] public bool IsGrounded;
        [NonSerialized] public bool IsJumping;
        [NonSerialized] public bool IsFalling;

        private void OnEnable()
        {
            var actions = _inputManager.Controls.Player;
            actions.Move.performed += OnMove;
            actions.Move.canceled  += OnMove;
            actions.Jump.performed += OnJump;
            actions.Jump.canceled  += OnJump;
            actions.Enable();
        }

        private void OnDisable()
        {
            var actions = _inputManager.Controls.Player;
            actions.Move.performed -= OnMove;
            actions.Move.canceled  -= OnMove;
            actions.Jump.performed -= OnJump;
            actions.Jump.canceled  -= OnJump;
            actions.Disable();
        }

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
            _rigidbody.linearVelocityX = _moveInput * moveSpeed;

            // Поворот спрайта
            if (Running)
            {
                Vector3 scale = _playerTransform.localScale;
                scale.x = _moveInput < 0f ? -1f : 1f;
                _playerTransform.localScale = scale;
            }
        }

        private void OnMove(InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.canceled ? 0f : ctx.ReadValue<float>();
            Running = _moveInput != 0;
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            // ставим флаг запроса прыжка
            _jumpRequest = !ctx.canceled;
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