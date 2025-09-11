using UnityEngine;

namespace World.Entities
{
    public abstract class EntityActivityTogglerAbstract : MonoBehaviour
    {
        public abstract void EnableActivity();
        public abstract void DisableActivity();
    }
}