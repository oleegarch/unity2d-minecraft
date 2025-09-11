using UnityEngine;

namespace World.Entities
{
    public class EntityPointsForMoving : MonoBehaviour
    {
        [SerializeField] private LayerMask _layerMask;
        [SerializeField] private Transform _tooLowPoint;
        [SerializeField] private Transform[] _canJumpPoints;
        [SerializeField] private Transform _needJumpPoint;

        public bool IsTooLowPoint()
        {
            // если в _tooLowPoint присутствует коллайдер
            // значит, мы не упадём с высока
            return !Physics2D.OverlapPoint(_tooLowPoint.position, _layerMask);
        }
        public bool CanJump()
        {
            // если в _canJumpPoints присутствуют коллайдеры,
            // значит мы не можем тут прыгать — так как мы ударимся об коллайдер
            foreach (Transform transform in _canJumpPoints)
            {
                if (Physics2D.OverlapPoint(transform.position, _layerMask))
                    return false;
            }

            return true;
        }
        public bool NeedJump()
        {
            // если в _needJumpPoint есть коллайдер,
            // значит нам нужно попробовать его перепрыгнуть если мы это сможем сделать относительно ещё CanJump
            return Physics2D.OverlapPoint(_needJumpPoint.position, _layerMask) && CanJump();
        }
    }
}