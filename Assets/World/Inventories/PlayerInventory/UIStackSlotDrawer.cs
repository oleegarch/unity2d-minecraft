using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace World.Inventories
{
    public class UIStackSlotDrawer : MonoBehaviour
    {
        [Header("UI references")]
        [SerializeField] private Image _uiItemImage;
        [SerializeField] private TextMeshProUGUI _uiTextCount;

        [NonSerialized] public ItemStack Stack;

        public void SetUp(ItemStack stack)
        {
            Stack = stack;

            if (stack?.Item?.Sprite != null)
            {
                _uiItemImage.sprite = stack.Item.Sprite;
                _uiItemImage.enabled = true;
            }
            else
            {
                _uiItemImage.enabled = false;
            }

            if (stack?.Count > 0)
            {
                _uiTextCount.SetText(stack.Count.ToString());
                _uiTextCount.enabled = true;
            }
            else
            {
                _uiTextCount.enabled = false;
            }
        }

        public void Dispose()
        {
            Stack = null;
            Destroy(gameObject);
        }
    }
}