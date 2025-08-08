using UnityEngine;

namespace World.InputActions
{
    public class WorldInputManager : MonoBehaviour
    {
        private WorldInputActions _controls;
        public WorldInputActions Controls => _controls ??= new WorldInputActions();
    }
}