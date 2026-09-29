using System;
using System.Collections.Generic;

using DreamVille.Configs;

namespace DreamVille.UI.Menus
{
    /// <summary>The level-select grid: one entry per level, in the order the view lists them.</summary>
    public interface ILevelSelectView
    {
        event Action<LevelDefinition> LevelChosen;

        IReadOnlyList<LevelDefinition> Levels { get; }

        /// <summary>Shows <paramref name="stars"/> earned stars on entry <paramref name="index"/>; a locked entry cannot be clicked.</summary>
        void ShowLevel(int index, int stars, bool isPlayable);
    }
}
