using System;
using System.Collections.Generic;
using UnityEngine;
using World.Items;

namespace World.Inventories
{
    public class UIItemCategorSlotsDrawer : MonoBehaviour
    {
        [SerializeField] private UIItemCategoriesDrawer _categoriesDrawer;
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Transform _slotsParent;

        private ItemDatabase _itemDatabase;
        private List<GameObject> _instantiatedSlots = new();

        public event Action<GameObject, ItemInfo> OnSlotCreated;
        public event Action<GameObject> OnSlotDestroy;

        private void OnEnable()
        {
            _categoriesDrawer.OnCategoryChanged += RecreateCategorySlots;
        }
        private void OnDisable()
        {
            _categoriesDrawer.OnCategoryChanged -= RecreateCategorySlots;
        }
        private void OnDestroy()
        {
            DestroyCategorySlots();
        }

        public void SetUp(ItemDatabase itemDatabase)
        {
            _itemDatabase = itemDatabase;
        }

        private void RecreateCategorySlots(ItemCategory category)
        {
            DestroyCategorySlots();

            foreach (ItemInfo info in _itemDatabase.items)
            {
                if (info.Category == category)
                {
                    GameObject go = Instantiate(_slotPrefab, _slotsParent);
                    _instantiatedSlots.Add(go);
                    OnSlotCreated?.Invoke(go, info);
                }
            }
        }

        public void DestroyCategorySlots()
        {
            foreach (var go in _instantiatedSlots)
            {
                OnSlotDestroy?.Invoke(go);
                Destroy(go);
            }
            _instantiatedSlots.Clear();
        }
        public void Dispose()
        {
            DestroyCategorySlots();
            Destroy(gameObject);
        }
    }
}