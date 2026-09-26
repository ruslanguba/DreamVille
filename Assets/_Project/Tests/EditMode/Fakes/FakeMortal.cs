using System;

using DreamVille.Gameplay.Damage;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written <see cref="IMortal"/> double: settable death state, counts attached handlers.</summary>
    public sealed class FakeMortal : IMortal
    {
        private Action<IMortal> _died;

        public event Action<IMortal> Died
        {
            add
            {
                _died += value;
                HandlerCount++;
            }
            remove
            {
                // Mirror delegate semantics: removing a handler that is not attached changes nothing.
                Action<IMortal> before = _died;
                _died -= value;
                if (!ReferenceEquals(before, _died))
                    HandlerCount--;
            }
        }

        public bool IsDead { get; set; }

        public int HandlerCount { get; private set; }

        /// <summary>Marks the fake dead and raises <see cref="Died"/>, even when it already was dead.</summary>
        public void Die()
        {
            IsDead = true;
            _died?.Invoke(this);
        }
    }
}
