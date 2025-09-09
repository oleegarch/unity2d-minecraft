using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using World.Inventories;

namespace World.Crafting
{
    [Serializable]
    public enum CraftIngredientType : byte
    {
        ExactlyItem,
        TypeItem
    }

    [Serializable]
    public class CraftIngredient
    {
        [Tooltip("Имя предмета для идентификации — Name в ItemInfo")]
        public string ItemName;
        [Tooltip("Тип предмета для крафта")]
        public string ItemType;

        [Tooltip("Количество предметов для крафта")]
        public int Quantity;
        [Tooltip("Как долго в секундах происходит крафт этого предмета")]
        public float Duration;

        [Tooltip("ExactlyItem — определённый предмет, TypeItem — любой предмет определённого типа")]
        public CraftIngredientType Type = CraftIngredientType.ExactlyItem;
    }

    [Serializable]
    public class CraftVariant
    {
        [Tooltip("Идентификатор варианта крафта этого предмета.")]
        public byte Id;

        [Tooltip("Тип инвентаря в котором будут происходить данные крафты.")]
        public InventoryType InventoryType;

        [Tooltip("Количество предметов на выходе с этого крафта.")]
        public int ReturnCount;

        [Tooltip("Какие ингредие́нты нужны для крафта предмета")]
        public List<CraftIngredient> Ingredients;
    }

    [Serializable]
    public class CraftVariants : IEnumerable<CraftVariant>
    {
        [Tooltip("Тип этого предмета. Для возможности крафта одного и того же предмета из предметов у которых такой же тип.")]
        public string ItemType;

        [Tooltip("Список возможных крафтов для этого предмета")]
        public List<CraftVariant> Variants;

        public List<CraftVariant> GetVariants(InventoryType inventoryType) => Variants.Where(v => v.InventoryType == inventoryType).ToList();
        public CraftVariant GetVariantById(int variantId) => Variants.First(v => v.Id == variantId);
        public bool IsAvailableFor(InventoryType inventoryType) => Variants.Any(v => v.InventoryType == inventoryType);

        public IEnumerator<CraftVariant> GetEnumerator() => Variants.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public CraftVariant this[int index] => Variants[index];
        public int Count => Variants.Count;
    }
}