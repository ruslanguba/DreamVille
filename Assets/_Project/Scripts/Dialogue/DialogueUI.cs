using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine;
using Zenject;

namespace DreamVille.Dialogue
{
    /// <summary>Displays the currently shown dialogue sentence and speaker icon.</summary>
    public class DialogueUI : MonoBehaviour
    {
        [FormerlySerializedAs("panel")]
        [SerializeField] private Image _panel;

        [FormerlySerializedAs("leftImage")]
        [SerializeField] private Image _leftImage;
        [FormerlySerializedAs("rightImage")]
        [SerializeField] private Image _rightImage;

        [FormerlySerializedAs("leftTextBackground")]
        [SerializeField] private Image _leftTextBackground;
        [FormerlySerializedAs("rightTextBackground")]
        [SerializeField] private Image _rightTextBackground;

        [FormerlySerializedAs("leftText")]
        [SerializeField] private TextMeshProUGUI _leftText;
        [FormerlySerializedAs("rightText")]
        [SerializeField] private TextMeshProUGUI _rightText;

        [FormerlySerializedAs("sizeOffset")]
        [SerializeField] private float _sizeOffset;

        private float _positionOffset;

        private Vector2 _originalSize;

        private Vector3 _leftOriginalPos;
        private Vector3 _rightOriginalPos;

        private IDialogueService _dialogue;

        [Inject]
        public void Construct(IDialogueService dialogue)
        {
            _dialogue = dialogue;
        }

        private void Awake()
        {
            _originalSize = _leftImage.rectTransform.sizeDelta;

            _leftOriginalPos = _leftImage.rectTransform.localPosition;
            _rightOriginalPos = _rightImage.rectTransform.localPosition;

            _panel.gameObject.SetActive(false);
        }

        public void SetLeftImage(Sprite sprite)
        {
            _leftImage.sprite = sprite;
        }

        public void ShowNextSentence(bool isPluh, string text)
        {
            if (isPluh)
            {
                _rightText.text = text;
                _leftTextBackground.gameObject.SetActive(false);
                _rightTextBackground.gameObject.SetActive(true);
            }
            else
            {
                _leftText.text = text;
                _leftTextBackground.gameObject.SetActive(true);
                _rightTextBackground.gameObject.SetActive(false);
            }
            ChangeIconsSize(isPluh);
        }

        private void ChangeIconsSize(bool isRight)
        {
            // Pick the selected and the other icon.
            RectTransform selected = isRight ? _rightImage.rectTransform : _leftImage.rectTransform;
            RectTransform other = isRight ? _leftImage.rectTransform : _rightImage.rectTransform;

            other.sizeDelta = _originalSize;
            selected.sizeDelta = _originalSize;

            _leftImage.rectTransform.localPosition = _leftOriginalPos;
            _rightImage.rectTransform.localPosition = _rightOriginalPos;

            // Grow the selected icon and offset it.
            selected.sizeDelta += Vector2.one * _sizeOffset;
            _positionOffset = _sizeOffset / 2;
            Vector3 newPos = selected.localPosition;
            newPos.y += _positionOffset;
            newPos.x += isRight ? -_positionOffset : _positionOffset;
            selected.localPosition = newPos;
        }

        public void OnNextButtonClick()
        {
            if (_dialogue == null)
            {
                Debug.LogError($"[{nameof(DialogueUI)}] {nameof(IDialogueService)} was not injected; create this prefab through the DI container.", this);
                return;
            }

            _dialogue.ShowNext();
        }

        public void ShowPanel()
        {
            _panel.gameObject.SetActive(true);
        }

        public void HidePanel()
        {
            _panel.gameObject.SetActive(false);
        }
    }
}
