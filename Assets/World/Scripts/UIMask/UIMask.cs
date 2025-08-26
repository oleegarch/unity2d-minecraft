using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace UIGlobal
{
    public class UIMask : MonoBehaviour
    {
        [SerializeField] private RawImage _maskImage;
        [SerializeField] private float _maxAlpha = 1f;
        [SerializeField] private float _fadeDuration = 1f;

        private Coroutine _currentFadeCoroutine;

        private void Awake()
        {
            if (_maskImage == null)
                _maskImage = GetComponent<RawImage>();
        }

        public void Appear()
        {
            ChangeAlpha(_maxAlpha);
        }

        public void Disappear()
        {
            ChangeAlpha(0f);
        }

        private void ChangeAlpha(float targetAlpha)
        {
            if (_currentFadeCoroutine != null)
                StopCoroutine(_currentFadeCoroutine);
            
            _currentFadeCoroutine = StartCoroutine(FadeAlphaCoroutine(targetAlpha, _fadeDuration));
        }

        private IEnumerator FadeAlphaCoroutine(float targetAlpha, float duration)
        {
            Color startColor = _maskImage.color;
            float startAlpha = startColor.a;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                _maskImage.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);
                yield return null;
            }

            _maskImage.color = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);
        }
    }
}