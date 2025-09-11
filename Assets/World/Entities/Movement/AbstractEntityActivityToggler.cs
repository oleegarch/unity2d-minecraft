using UnityEngine;

namespace World.Entities
{
    public abstract class AbstractEntityActivityToggler : MonoBehaviour
    {
        public abstract void EnableActivity();
        public abstract void DisableActivity();
    }
}