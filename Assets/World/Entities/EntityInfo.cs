using UnityEngine;

namespace World.Entities
{
    [CreateAssetMenu(menuName = "Entity/New EntityInfo")]
    public class EntityInfo : ScriptableObject
    {
        [Tooltip("Идентификатор сущности. Рассчитывается автоматически.")]
        public ushort Id;

        [Tooltip("Имя сущности для удобной идентификации.")]
        public string Name;

        [Tooltip("Название сущности которое видит пользователь.")]
        public string Title;

        [Tooltip("Префаб сущености для создания.")]
        public GameObject Prefab;

        [Tooltip("Высота коллайдера столкновений.")]
        public float ColliderHeight;
    }
}