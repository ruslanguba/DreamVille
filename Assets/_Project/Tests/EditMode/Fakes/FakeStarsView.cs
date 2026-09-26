using System.Collections.Generic;

using DreamVille.UI.Hud;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Records star fills and unlock effects in call order.</summary>
    public sealed class FakeStarsView : IStarsView
    {
        private readonly List<(int StarIndex, float Normalized)> _progress = new List<(int, float)>();
        private readonly List<int> _unlocked = new List<int>();

        public IReadOnlyList<(int StarIndex, float Normalized)> Progress => _progress;
        public IReadOnlyList<int> Unlocked => _unlocked;

        public void SetProgress(int starIndex, float normalized)
        {
            _progress.Add((starIndex, normalized));
        }

        public void PlayUnlock(int starIndex)
        {
            _unlocked.Add(starIndex);
        }
    }
}
