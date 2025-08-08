using UnityEngine;
using System;

namespace World
{
    public class WorldController : MonoBehaviour
    {
        public event Action OnWorldInited;
        public event Action OnWorldDestroyed;

        private void Awake()
        {
            OnWorldInited?.Invoke();
        }

        private void OnDestroy()
        {
            OnWorldDestroyed?.Invoke();
        }
    }
}