using UnityEngine;
using World.Crafting;

namespace World.Items
{
    [CreateAssetMenu(menuName = "Items/New ItemInfo")]
    public class ItemInfo : ScriptableObject
    {
        [Tooltip("Идентификатор предмета. Рассчитывается автоматически.")]
        public ushort Id;

        [Tooltip("Имя предмета для удобной идентификации.")]
        public string Name;

        [Tooltip("Название предмета который видит пользователь.")]
        public string Title;

        [Tooltip("Какой блок относится к этому предмету. Его Id.")]
        public ushort BlockId;

        [Tooltip("Спрайт предмета. Может отличаться от спрайта блока относящегося к этому предмету.")]
        public Sprite Sprite;

        [Tooltip("Категория предмета. Используется в режиме креатива для удобного поиска предметов.")]
        public ItemCategory Category;

        [Tooltip("Максимальное количество предметов в одном стаке. Для орудий устанавливается 1.")]
        public ushort MaxStack = 100;

        [Tooltip("Из чего этот предмет создаётся в верстаке.")]
        public CraftVariants CraftVariants;
    }
}