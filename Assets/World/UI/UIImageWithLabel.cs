using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace World.UI
{
    public class UIImageWithLabel : MonoBehaviour
    {
        [SerializeField] private Image _image;
        [SerializeField] private TextMeshProUGUI _label;
        [SerializeField] private TextMeshProUGUI _additionalLabel;
        [SerializeField] private float _changingAnimationDuration = 1.5f;

        public void SetUp(Sprite sprite, string label, string additionalLabel = null)
        {
            _image.enabled = sprite != null;
            _image.sprite = sprite;
            _label.enabled = !string.IsNullOrEmpty(label);
            _label.SetText(label);

            if (_additionalLabel != null)
            {
                _additionalLabel.enabled = !string.IsNullOrEmpty(additionalLabel);
                _additionalLabel.SetText(additionalLabel);
            }
        }

        public Sprite GetCurrentImageSprite() => _image.sprite;
        public string GetCurrentLabelText() => _label.text;

        public void ToggleLabel(bool active)
        {
            _label.gameObject.SetActive(active);
        }

        public void StartChangingAnimation(List<Sprite> sprites, List<string> labels)
        {
            StartCoroutine(ChangingAnimation(sprites, labels));
        }

        public IEnumerator ChangingAnimation(List<Sprite> sprites, List<string> labels)
        {
            _image.enabled = true;
            _label.enabled = true;
            
            int index = 0;
            while (true)
            {
                _image.sprite = sprites[index];
                _label.SetText(labels[index]);

                yield return new WaitForSeconds(_changingAnimationDuration);

                index++;

                if (index > sprites.Count - 1)
                    index = 0;
            }
        }
    }
}