using System.Collections.Generic;

using DreamVille.UI.Menus;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Records every locale code the presenter selected, in order.</summary>
    public sealed class FakeLocaleSelector : ILocaleSelector
    {
        private readonly List<string> _selected = new List<string>();

        public IReadOnlyList<string> Selected => _selected;

        public void Select(string localeCode)
        {
            _selected.Add(localeCode);
        }
    }
}
