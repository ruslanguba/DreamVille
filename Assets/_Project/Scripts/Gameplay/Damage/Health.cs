using System;

namespace DreamVille.Gameplay.Damage
{
    /// <summary>
    /// Pure hit-point model shared by anything that can take damage and die exactly once.
    /// Damage after death is ignored, so only one <see cref="TakeDamage"/> call ever reports the kill.
    /// </summary>
    public sealed class Health
    {
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
            return IsDead;
        }
    }
}
