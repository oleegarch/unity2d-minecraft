using UnityEngine;

namespace World.Entities
{
    public class EntityActivityToggler : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private Animator _animator;
        [SerializeField] private EntityActivityTogglerAbstract[] _components;
        [SerializeField] private bool _disableInAwake = true;

        private void Awake()
        {
            if (_disableInAwake)
                Disable();
        }

        public void Enable()
        {
            _rigidbody.simulated = true;
            _animator.enabled = true;

            foreach (var component in _components)
                component.EnableActivity();
        }

        public void Disable()
        {
            _rigidbody.simulated = false;
            _animator.enabled = false;

            foreach (var component in _components)
                component.DisableActivity();
        }
    }
}