using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace World.Inventories
{
    public class UIItemSlotAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _shadow;

        private void Awake()
        {
            _shadow.enabled = false;
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            _shadow.enabled = true;
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            _shadow.enabled = false;
        }
    }
}