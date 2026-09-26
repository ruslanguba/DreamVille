using System.Collections.Generic;

using DreamVille.Gameplay.NPC;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written <see cref="INpcAnimatorParameters"/> double: records every bool write in call order.</summary>
    public sealed class FakeNpcAnimatorParameters : INpcAnimatorParameters
    {
        private readonly List<(int Hash, bool Value)> _writes = new List<(int Hash, bool Value)>();
        private readonly Dictionary<int, bool> _values = new Dictionary<int, bool>();

        public IReadOnlyList<(int Hash, bool Value)> Writes => _writes;

        public void SetBool(int parameterHash, bool value)
        {
            _writes.Add((parameterHash, value));
            _values[parameterHash] = value;
        }

        public bool WasWritten(int parameterHash) => _values.ContainsKey(parameterHash);

        /// <summary>Last value written to <paramref name="parameterHash"/>; false if it was never written.</summary>
        public bool ValueOf(int parameterHash) => _values.TryGetValue(parameterHash, out bool value) && value;

        public void ClearWrites()
        {
            _writes.Clear();
            _values.Clear();
        }
    }
}
