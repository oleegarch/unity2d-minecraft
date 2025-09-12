using UnityEngine;

namespace World.Entities
{
    [RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(Animator))]
    public class EntityGroundCheck : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Animator _animator;

        [Tooltip("Отправлять bool значения IsGrounded, IsJumping и IsFalling в Animator")]
        [SerializeField] private bool _sendToAnimator = false;

        [Header("Movement Settings")]
        [SerializeField] private float measureVelocityY = 0.1f;

        [Header("Ground Check Position Settings")]
        [SerializeField] private LayerMask groundCheckLayer;
        [SerializeField] private Transform groundCheckTransform;
        [SerializeField] private Vector2 groundCheckPositionSize;
        [SerializeField] private float groundCheckPositionAngle;

        public bool IsGrounded;
        public bool IsJumping;
        public bool IsFalling;

        private void Update()
        {
            IsGrounded = Physics2D.OverlapBox((Vector2)groundCheckTransform.position, groundCheckPositionSize, groundCheckPositionAngle, groundCheckLayer);
            IsJumping = _rigidbody.linearVelocityY > measureVelocityY;
            IsFalling = _rigidbody.linearVelocityY < -measureVelocityY;
            
            if (_sendToAnimator)
            {
                _animator.SetBool("IsGrounded", IsGrounded);
                _animator.SetBool("IsJumping", IsJumping);
                _animator.SetBool("IsFalling", IsFalling);
            }
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