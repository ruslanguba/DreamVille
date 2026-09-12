using System;

namespace DreamVille.Gameplay.Damage
{
    /// <summary>
    /// Pure hit-point model shared by anything that can take damage and die exactly once.
    /// Guards against damage being applied after death, so a second hit can never re-fire
    /// <see cref="Died"/>.
    /// </summary>
    public sealed class Health
    {
        /// <summary>Raised with the new current value whenever damage is applied.</summary>
        public event Action<float> Changed;

        /// <summary>Raised once, the moment <see cref="Current"/> reaches zero.</summary>
        public event Action Died;

        public Health(float max)
        {
            if (max <= 0f)
                throw new ArgumentOutOfRangeException(nameof(max), max, "Max health must be greater than zero.");

            Max = max;
            Current = max;
        }

        public float Max { get; }

        public float Current { get; private set; }

        public bool IsDead => Current <= 0f;

        /// <summary>Applies damage. Returns true when this call is the one that killed it.</summary>
        public bool TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
                return false;

            Current = Math.Max(0f, Current - amount);
            Changed?.Invoke(Current);

            if (!IsDead)
                return false;

            Died?.Invoke();
            return true;
        }
    }
}
