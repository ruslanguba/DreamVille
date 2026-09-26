using System.Collections.Generic;

using DreamVille.UI.Hud;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Records every score the presenter asked to show.</summary>
    public sealed class FakeScoreView : IScoreView
    {
        private readonly List<float> _shown = new List<float>();

        public IReadOnlyList<float> Shown => _shown;

        public void ShowAdded(float delta)
        {
            _shown.Add(delta);
        }
    }
}
