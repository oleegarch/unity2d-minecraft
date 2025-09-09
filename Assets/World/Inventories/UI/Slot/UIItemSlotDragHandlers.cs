using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace World.Inventories
{
    public class UIItemSlotDragHandlers : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event Action<Vector3> StartDrag;
        public event Action<Vector3> ContinueDrag;
        public event Action FinishDrag;

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (UIItemSlotDragger.DraggingByClick) return;
            StartDrag?.Invoke(eventData.position);
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (UIItemSlotDragger.DraggingByClick) return;
            ContinueDrag?.Invoke(eventData.position);
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            if (UIItemSlotDragger.DraggingByClick) return;
            FinishDrag?.Invoke();
        }
    }
}