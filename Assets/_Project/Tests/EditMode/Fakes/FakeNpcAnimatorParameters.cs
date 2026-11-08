using System.Collections.Generic;

using DreamVille.Gameplay.Npc;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written <see cref="INpcAnimatorParameters"/> double: keeps the last bool written to each parameter.</summary>
    public sealed class FakeNpcAnimatorParameters : INpcAnimatorParameters
    {
        private readonly Dictionary<int, bool> _values = new Dictionary<int, bool>();

        public void SetBool(int parameterHash, bool value)
        {
            _values[parameterHash] = value;
        }

        public bool WasWritten(int parameterHash) => _values.ContainsKey(parameterHash);

        /// <summary>Last value written to <paramref name="parameterHash"/>; false if it was never written.</summary>
        public bool ValueOf(int parameterHash) => _values.TryGetValue(parameterHash, out bool value) && value;

        public void ClearWrites()
        {
            _values.Clear();
        }
    }
}
