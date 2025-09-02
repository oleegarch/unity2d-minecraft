using System;
using UnityEngine;

namespace World.Inventories
{
    [Serializable]
    public enum InventoryType : ushort
    {
        None,
        Inventory,
        CraftingTable
    }

    [Serializable]
    public class InventoryIdentifier
    {
        [Tooltip("Тип инвентаря который будет открыт при клике по этому блоку")]
        public InventoryType Type = InventoryType.None;

        [Tooltip("Количество слотов инвентаря (для сундуков)")]
        public int SlotCount;
    }
}