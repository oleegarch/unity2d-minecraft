using UnityEngine;

namespace World.Entities.Player
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private EntityMovement _movement;

        public void AnimateItemThrow(Vector3 toPosition)
        {
            _animator.SetTrigger("Throw");
            FlipTo(toPosition);
        }

        public void AnimateBlockBreaking(WorldPosition breakingPosition)
        {
            _animator.SetBool("Breaking", true);
            FlipTo(breakingPosition.ToVector3Int());
        }
        public void StopBlockBreaking()
        {
            _animator.SetBool("Breaking", false);
        }

        public void FlipTo(Vector3 toPosition)
        {
            if (!_movement.Running)
            {
                Vector3 offset = toPosition - transform.position;
                _movement.Flip(Mathf.Sign(offset.x));
            }
        }
    }
}