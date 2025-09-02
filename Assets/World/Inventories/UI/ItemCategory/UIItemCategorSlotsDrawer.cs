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

        public event Action<UIItemSlotDrawer, UIItemSlotDragger> OnSlotCreated;
        public event Action<UIItemSlotDrawer, UIItemSlotDragger> OnSlotDestroy;

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

                    // Визуализируем слот
                    var drawer = go.GetComponent<UIItemSlotDrawer>();
                    var stack = new ItemStack(info, info.MaxStack);
                    drawer.SetUp(stack, _itemDatabase);

                    // Подписываемся на событие перетаскивания с креативного инвентаря
                    var dragger = go.GetComponent<UIItemSlotDragger>();

                    OnSlotCreated?.Invoke(drawer, dragger);

                    _instantiatedSlots.Add(go);
                }
            }
        }

        public void DestroyCategorySlots()
        {
            foreach (var go in _instantiatedSlots)
            {
                OnSlotDestroy?.Invoke(go.GetComponent<UIItemSlotDrawer>(), go.GetComponent<UIItemSlotDragger>());
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