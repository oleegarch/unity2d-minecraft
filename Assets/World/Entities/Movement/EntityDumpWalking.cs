using System.Collections;
using UnityEngine;

namespace World.Entities
{
    public class EntityDumpWalking : MonoBehaviour
    {
        [SerializeField] private EntityPointsForMoving _ability;
        [SerializeField] private EntityMovement _movement;

        [Header("Настройки ходьбы")]
        [Tooltip("Множитель скорости ходьбы")]
        [SerializeField] private MinMax _walkSpeedMultiplier = new MinMax(1f, 1.5f);

        [Tooltip("Длительность ходьбы")]
        [SerializeField] private MinMax _walkDuration = new MinMax(2f, 5f);

        [Tooltip("Длительность постоять")]
        [SerializeField] private MinMax _walkDelay = new MinMax(5f, 10f);

        [Tooltip("Продлить длительность ходьбы при прыжке")]
        [SerializeField] private float _walkIncrementAfterJump = 1f;

        private Coroutine _walking;

        private void OnEnable()
        {
            _walking = StartCoroutine(DumpWalking());
        }
        private void OnDisable()
        {
            StopCoroutine(_walking);
            _walking = null;
        }

        private IEnumerator DumpWalking()
        {
            while (true)
            {
                float walkSpeedMultiplier = _walkSpeedMultiplier.GetRandom();
                float walkDuration = _walkDuration.GetRandom();
                float walkDelay = _walkDelay.GetRandom();
                float direction = Random.value < 0.5f ? 1f : -1f; // 1 = вправо, -1 = влево

                _movement.Move(0f);
                yield return new WaitForSeconds(walkDelay);

                _movement.MoveSpeed(walkSpeedMultiplier);
                _movement.Move(direction);

                float spent = 0f;
                while (spent < walkDuration)
                {
                    if (_ability.NeedJump())
                    {
                        _movement.Jump();
                        spent -= _walkIncrementAfterJump;
                    }
                    // если в процессе ходьбы мы дошли до какого-то обрыва
                    // останавливаемся на walkDelay и меняем направление ходьбы
                    else if (_ability.IsTooLowPoint())
                    {
                        spent = 0f;
                        direction *= -1f;

                        _movement.Move(0f);
                        yield return new WaitForSeconds(walkDelay);

                        _movement.MoveSpeed(walkSpeedMultiplier);
                        _movement.Move(direction);
                    }

                    yield return new WaitForFixedUpdate();

                    spent += Time.fixedDeltaTime;
                }
            }
        }
    }
}