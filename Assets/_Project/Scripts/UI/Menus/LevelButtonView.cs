using System;
using UnityEngine;
using UnityEngine.UI;

using DreamVille.Configs;

namespace DreamVille.UI.Menus
{
    /// <summary>
    /// One entry of the level-select grid: the level it stands for, its button and its star icons.
    /// Passive: <see cref="LevelSelectView"/> aggregates the entries, the presenter decides what they show.
    /// </summary>
    public sealed class LevelButtonView : MonoBehaviour
    {
        [SerializeField] private LevelDefinition _level;
        [SerializeField] private Button _button;
        [SerializeField] private Image[] _stars;

        /// <summary>Raised with <see cref="Level"/> when the player clicks the entry.</summary>
        public event Action<LevelDefinition> Clicked;

        public LevelDefinition Level => _level;

        private void Awake()
        {
            if (_button == null)
                Debug.LogError($"[{nameof(LevelButtonView)}] {nameof(_button)} is not assigned.", this);
        }

        private void OnEnable()
        {
            if (_button != null)
                _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnDisable()
        {
            if (_button != null)
                _button.onClick.RemoveListener(OnButtonClicked);
        }

        /// <summary>Lights the first <paramref name="stars"/> star icons; a locked entry cannot be clicked.</summary>
        public void Show(int stars, bool isPlayable)
        {
            for (int i = 0; i < _stars.Length; i++)
                _stars[i].gameObject.SetActive(i < stars);

            _button.interactable = isPlayable;
        }

        private void OnButtonClicked()
        {
            Clicked?.Invoke(_level);
        }
    }
}
