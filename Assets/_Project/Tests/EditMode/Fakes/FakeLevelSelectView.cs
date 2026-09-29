using System;
using System.Collections.Generic;

using DreamVille.Configs;
using DreamVille.UI.Menus;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// A level grid over the levels the test passes in. Records every entry the presenter showed
    /// and raises <see cref="LevelChosen"/> on request.
    /// </summary>
    public sealed class FakeLevelSelectView : ILevelSelectView
    {
        private readonly List<(int Index, int Stars, bool IsPlayable)> _shown = new List<(int, int, bool)>();

        public FakeLevelSelectView(params LevelDefinition[] levels)
        {
            Levels = levels;
        }

        public event Action<LevelDefinition> LevelChosen;

        public IReadOnlyList<LevelDefinition> Levels { get; }

        public IReadOnlyList<(int Index, int Stars, bool IsPlayable)> Shown => _shown;

        public void ShowLevel(int index, int stars, bool isPlayable)
        {
            _shown.Add((index, stars, isPlayable));
        }

        public void Choose(LevelDefinition level)
        {
            LevelChosen?.Invoke(level);
        }
    }
}
