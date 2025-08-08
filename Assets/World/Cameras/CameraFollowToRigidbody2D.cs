using UnityEngine;

namespace World.Cameras
{
    [RequireComponent(typeof(CameraObserver))]
    public class CameraFollowToRigidbody2D : MonoBehaviour
    {
        [SerializeField] private CameraObserver _observer;
        [SerializeField] private Rigidbody2D _targetRigidbody;
        [SerializeField] private float _followSpeed = 5f;
        [SerializeField] private Vector3 _offset = new(0, 0, -10);

        private void LateUpdate()
        {
            if (_targetRigidbody == null) return;

            // Целевая позиция камеры исходя из Rigidbody2D.position
            Vector3 targetPosition = new Vector3(_targetRigidbody.position.x, _targetRigidbody.position.y, 0f) + _offset;

            // Плавно движем камеру
            _observer.SetPosition(Vector3.Lerp(
                _observer.GetPosition(),
                targetPosition,
                _followSpeed * Time.deltaTime
            ));
        }
    }
}