using System.Collections.Generic;

using DreamVille.Gameplay.NPC;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written <see cref="INpcReactions"/> double: records every reaction by method name, in call order.</summary>
    public sealed class FakeNpcReactions : INpcReactions
    {
        private readonly List<string> _calls = new List<string>();

        public IReadOnlyList<string> Calls => _calls;

        public void SetGreeting() => _calls.Add(nameof(SetGreeting));

        public void SetTalk() => _calls.Add(nameof(SetTalk));

        public void SetApplause() => _calls.Add(nameof(SetApplause));

        public void SetIdle() => _calls.Add(nameof(SetIdle));

        public void ClearCalls() => _calls.Clear();
    }
}
