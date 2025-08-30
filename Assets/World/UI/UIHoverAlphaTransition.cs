using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UIGlobal
{
    [RequireComponent(typeof(Graphic))]
    public class UIHoverColorTransition : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.gray;
        [SerializeField] private float transitionDuration = 0.2f;

        private Graphic _graphic;
        private Coroutine _transitionRoutine;

        private void Awake()
        {
            _graphic = GetComponent<Graphic>();
            _graphic.color = normalColor;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StartColorTransition(hoverColor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartColorTransition(normalColor);
        }

        private void StartColorTransition(Color targetColor)
        {
            if (_transitionRoutine != null)
                StopCoroutine(_transitionRoutine);

            _transitionRoutine = StartCoroutine(AnimateColor(targetColor));
        }

        private IEnumerator AnimateColor(Color targetColor)
        {
            Color startColor = _graphic.color;
            float time = 0f;

            while (time < transitionDuration)
            {
                time += Time.unscaledDeltaTime;
                float t = time / transitionDuration;
                _graphic.color = Color.Lerp(startColor, targetColor, t);
                yield return null;
            }

            _graphic.color = targetColor;
            _transitionRoutine = null;
        }
    }
}