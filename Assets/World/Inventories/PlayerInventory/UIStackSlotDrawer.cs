using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using World.Items;

namespace World.Inventories
{
    public class UIStackSlotDrawer : MonoBehaviour
    {
        [Header("UI references")]
        [SerializeField] private Image _uiItemImage;
        [SerializeField] private TextMeshProUGUI _uiTextCount;

        [NonSerialized] public ItemStack Stack;

        public void SetUp(ItemStack stack, ItemDatabase itemDatabase)
        {
            Stack = stack;

            Sprite itemSprite = stack.Instance?.GetItemInfo(itemDatabase).Sprite;
            if (itemSprite != null)
            {
                _uiItemImage.sprite = itemSprite;
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