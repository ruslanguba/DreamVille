using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DreamVille.UI.Dialogues
{
    /// <summary>
    /// The dialogue canvas: a panel with a speaker icon and a text cloud on each side. Passive: shows what
    /// <see cref="DialoguePresenter"/> asks for and raises <see cref="NextRequested"/> for the Next button,
    /// bound in code.
    /// </summary>
    public sealed class DialogueView : MonoBehaviour, IDialogueView
    {
        [SerializeField] private Image _panel;

        [SerializeField] private Image _leftImage;
        [SerializeField] private Image _rightImage;

        [SerializeField] private Image _leftTextBackground;
        [SerializeField] private Image _rightTextBackground;

        [SerializeField] private TextMeshProUGUI _leftText;
        [SerializeField] private TextMeshProUGUI _rightText;

        [Tooltip("Extra size, in canvas units, added to both axes of the icon of the current speaker.")]
        [SerializeField] private float _sizeOffset;

        [SerializeField] private Button _nextButton;

        private Vector2 _originalSize;

        private Vector3 _leftOriginalPosition;
        private Vector3 _rightOriginalPosition;

        public event Action NextRequested;

        private void Awake()
        {
            if (_nextButton == null)
                Debug.LogError($"[{nameof(DialogueView)}] {nameof(_nextButton)} is not assigned.", this);

            _originalSize = _leftImage.rectTransform.sizeDelta;

            _leftOriginalPosition = _leftImage.rectTransform.localPosition;
            _rightOriginalPosition = _rightImage.rectTransform.localPosition;

            _panel.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (_nextButton != null)
                _nextButton.onClick.AddListener(OnNextClicked);
        }

        private void OnDisable()
        {
            if (_nextButton != null)
                _nextButton.onClick.RemoveListener(OnNextClicked);
        }

        public void Show()
        {
            _panel.gameObject.SetActive(true);
        }

        public void Hide()
        {
            // A dialogue cut short by unloading the scene ends after the canvas may already be destroyed.
            if (_panel == null)
                return;

            _panel.gameObject.SetActive(false);
        }

        public void ShowLine(bool speakerOnRight, string text)
        {
            if (speakerOnRight)
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
            ChangeIconsSize(speakerOnRight);
        }

        private void OnNextClicked()
        {
            NextRequested?.Invoke();
        }

        private void ChangeIconsSize(bool isRight)
        {
            RectTransform selected = isRight ? _rightImage.rectTransform : _leftImage.rectTransform;
            RectTransform other = isRight ? _leftImage.rectTransform : _rightImage.rectTransform;

            other.sizeDelta = _originalSize;
            selected.sizeDelta = _originalSize;

            _leftImage.rectTransform.localPosition = _leftOriginalPosition;
            _rightImage.rectTransform.localPosition = _rightOriginalPosition;

            selected.sizeDelta += Vector2.one * _sizeOffset;

            // With a centred pivot the icon grows by half the offset on each side; shifting it by that half
            // keeps its outer bottom corner where it was, so only the top and inner edges move.
            float positionOffset = _sizeOffset / 2f;
            Vector3 newPos = selected.localPosition;
            newPos.y += positionOffset;
            newPos.x += isRight ? -positionOffset : positionOffset;
            selected.localPosition = newPos;
        }
    }
}
