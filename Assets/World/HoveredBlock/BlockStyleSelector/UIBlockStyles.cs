using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using World.Blocks;

using Image = UnityEngine.UI.Image;

namespace World.HoveredBlock.BlockStylesSelector
{
    public class UIBlockStyles : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private float _size = 100f;
        [SerializeField] private Color32 _activeColor = new Color32(0, 255, 255, 255);
        [SerializeField] private Color32 _inactiveColor = new Color32(0, 0, 0, 123);
        [SerializeField] private Color32 _blockColor = new Color32(255, 255, 255, 255);
        [SerializeField] private Color32 _blockBehindColor = new Color32(0, 0, 0, 50);
        [SerializeField] private Image _image;
        [SerializeField] private Image _blockImage;
        [SerializeField] private Outline _blockOutline;

        [NonSerialized] public bool IsHovered;
        [NonSerialized] public int Index;
        [NonSerialized] public BlockPlacementVariant Variant;

        public void Initialize(BlockInfo blockInfo, BlockPlacementVariant variant, int index)
        {
            Index = index;
            Variant = variant;

            _blockImage.sprite = blockInfo.Sprite;

            _blockOutline.enabled = variant.StylesOverrides.HasCollider;
            _blockImage.color = variant.StylesOverrides.IsBehind ? _blockBehindColor : _blockColor;

            ChangeBackground();
        }

        public void Centering(int count)
        {
            var rt = transform as RectTransform;
            if (rt == null) return;

            // Общая ширина всех элементов
            float totalWidth = count * _size;

            // Центр родителя — 0. Нам нужно сдвинуть каждый элемент так, 
            // чтобы весь ряд оказался по центру.
            // Начальная позиция для элемента с индексом 0:
            float startX = -(totalWidth - _size) / 2f;

            // Позиция текущего элемента
            float posX = startX + Index * _size;

            rt.anchoredPosition = new Vector2(posX, rt.anchoredPosition.y);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsHovered = true;
            ChangeBackground();
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            IsHovered = false;
            ChangeBackground();
        }

        private void ChangeBackground()
        {
            _image.color = IsHovered ? _activeColor : _inactiveColor;
        }
    }
}