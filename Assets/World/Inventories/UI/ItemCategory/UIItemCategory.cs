using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using World.Items;

namespace World.Inventories
{
    public class UIItemCategory : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _categoryImage;
        [SerializeField] private Image _itemImage;
        [SerializeField] private Color32 _activeColor = new Color32(226, 26, 26, 255);
        [SerializeField] private Color32 _inactiveColor = new Color32(233, 25, 14, 255);

        private ItemCategoryInfo _categoryInfo;
        public event Action<ItemCategory> OnActiveSetAttempt;

        public void SetUp(ItemCategoryInfo info)
        {
            _categoryInfo = info;
            _itemImage.sprite = info.Sprite;
        }

        public void SetActive(ItemCategory newCategory)
        {
            SetActive(newCategory == _categoryInfo.Category);
        }
        public void SetActive(bool active)
        {
            _categoryImage.color = active ? _activeColor : _inactiveColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnActiveSetAttempt?.Invoke(_categoryInfo.Category);
        }
    }
}