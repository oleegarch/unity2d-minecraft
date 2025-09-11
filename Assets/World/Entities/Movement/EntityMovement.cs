using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

        public float Flip => _moveVelocity < 0f ? -1f : 1f;
        public bool Running => _moveVelocity != 0f;
        public float RunningSpeed => _moveVelocity * moveSpeed * _moveSpeedMultiplier;

        private void OnEnable()
        {
            _animator.SetBool("Running", Running);
            _animator.SetFloat("RunningSpeed", RunningSpeed);
        }

        private void FixedUpdate()
        {
            // Запускаем физический прыжок, если запросили и *стоим на земле*
            // * — кастомно
            if (_jumpRequest && (_groundCheck == null || _groundCheck.IsGrounded))
            {
                Jump();
            }

            // Движение по горизонтали
            _rigidbody.linearVelocityX = RunningSpeed;

            // Поворот спрайта
            if (Running && _playerTransform.localScale.x != Flip)
            {
                Vector3 scale = _playerTransform.localScale;
                scale.x = Flip;
                _playerTransform.localScale = scale;
            }

#if UNITY_EDITOR
            // Проверка, выделен ли именно этот объект
            if (Selection.activeGameObject == gameObject)
            {
                Debug.Log($"{name} info: Running:{Running}, RunningSpeed:{RunningSpeed}, _moveVelocity:{_moveVelocity}, velocityX:{_rigidbody.linearVelocityX}");
            }
#endif
        }

        public void Move(float move)
        {
            _moveVelocity = move;
            _animator.SetBool("Running", Running);
            _animator.SetFloat("RunningSpeed", RunningSpeed);
        }
        public void MoveSpeed(float speed)
        {
            _moveSpeedMultiplier = speed;
            _animator.SetFloat("RunningSpeed", RunningSpeed);
        }
        public void Jump(bool requested)
        {
            _jumpRequest = requested;
        }
        public void Jump()
        {
            _rigidbody.linearVelocityY = jumpForce;
        }
    }
}