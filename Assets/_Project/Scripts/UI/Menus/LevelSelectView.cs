using System;
using System.Collections.Generic;
using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.UI.Menus
{
    /// <summary>
    /// The level-select grid: aggregates its <see cref="LevelButtonView"/> entries, listed in
    /// <see cref="SceneCatalog"/> order, and forwards their clicks as <see cref="LevelChosen"/>.
    /// </summary>
    public sealed class LevelSelectView : MonoBehaviour, ILevelSelectView
    {
        [SerializeField] private LevelButtonView[] _buttons = Array.Empty<LevelButtonView>();

        public event Action<LevelDefinition> LevelChosen;

        public IReadOnlyList<LevelDefinition> Levels
        {
            get
            {
                var levels = new LevelDefinition[_buttons.Length];
                for (int i = 0; i < _buttons.Length; i++)
                    levels[i] = _buttons[i] != null ? _buttons[i].Level : null;

                return levels;
            }
        }

        private void OnEnable()
        {
            foreach (LevelButtonView button in _buttons)
            {
                if (button != null)
                    button.Clicked += OnButtonClicked;
            }
        }

        private void OnDisable()
        {
            foreach (LevelButtonView button in _buttons)
            {
                if (button != null)
                    button.Clicked -= OnButtonClicked;
            }
        }

        public void ShowLevel(int index, int stars, bool isPlayable)
        {
            LevelButtonView button = _buttons[index];
            if (button == null)
            {
                Debug.LogError($"[{nameof(LevelSelectView)}] Entry {index} has no {nameof(LevelButtonView)} assigned.", this);
                return;
            }

            button.Show(stars, isPlayable);
        }

        private void OnButtonClicked(LevelDefinition level)
        {
            LevelChosen?.Invoke(level);
        }
    }
}
