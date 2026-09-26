using System.Collections.Generic;

using DreamVille.UI.Hud;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Records every pillow row the presenter asked to draw.</summary>
    public sealed class FakePillowView : IPillowView
    {
        private readonly List<(int Total, int InHand)> _shown = new List<(int, int)>();

        public IReadOnlyList<(int Total, int InHand)> Shown => _shown;

        public void Show(int total, int inHand)
        {
            _shown.Add((total, inHand));
        }
    }
}
