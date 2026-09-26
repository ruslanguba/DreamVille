using System.Collections.Generic;

using DreamVille.UI.Hud;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Records every remaining-enemies value the presenter asked to show.</summary>
    public sealed class FakeEnemyCounterView : IEnemyCounterView
    {
        private readonly List<int> _shown = new List<int>();

        public IReadOnlyList<int> Shown => _shown;

        public void SetRemaining(int remaining)
        {
            _shown.Add(remaining);
        }
    }
}
