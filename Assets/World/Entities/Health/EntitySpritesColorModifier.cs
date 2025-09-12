using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World.Entities
{
    public class EntitySpritesColorModifier : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _bodySprite;
        [SerializeField] private SpriteRenderer _headSprite;
        [SerializeField] private SpriteRenderer[] _limbSprites;

        private IEnumerable<SpriteRenderer> _allSprites;
        private Dictionary<SpriteRenderer, Coroutine> _currentFadeColorAnimations = new();

        private void Awake()
        {
            _allSprites = _limbSprites.Append(_bodySprite).Append(_headSprite);
        }

        public void FadeHead(Color color)
        {
            AnimateFade(_headSprite, color);
        }
        public void FadeBody(Color color)
        {
            AnimateFade(_bodySprite, color);
        }
        public void FadeLimbs(Color color)
        {
            foreach (SpriteRenderer sprite in _limbSprites)
                AnimateFade(sprite, color);
        }
        public void FadeAll(Color color)
        {
            foreach (SpriteRenderer sprite in _allSprites)
                AnimateFade(sprite, color);
        }

        private void AnimateFade(SpriteRenderer sprite, Color color)
        {
            DisposeFade(sprite);

            Coroutine fade = StartCoroutine(AnimateFadeColor(sprite, color, Color.white));
            _currentFadeColorAnimations.Add(sprite, fade);
        }
        private IEnumerator AnimateFadeColor(SpriteRenderer sprite, Color fromColor, Color toColor, float duration = 0.2f)
        {
            sprite.color = fromColor;

            float spent = 0f;
            while (spent < duration)
            {
                float progress = spent / duration;
                sprite.color = Color.Lerp(fromColor, toColor, progress);

                yield return null;

                spent += Time.deltaTime;
            }

            DisposeFade(sprite);
        }
        private void DisposeFade(SpriteRenderer sprite)
        {
            if (_currentFadeColorAnimations.TryGetValue(sprite, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
                _currentFadeColorAnimations.Remove(sprite);
            }
        }
    }
}