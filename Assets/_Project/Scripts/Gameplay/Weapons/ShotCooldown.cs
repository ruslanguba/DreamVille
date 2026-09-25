using System;

namespace DreamVille.Gameplay.Weapons
{
    /// <summary>
    /// Timestamp cooldown between shots. There is no per-frame countdown: the caller passes the current time,
    /// so tests control it and a scaled clock (pause) freezes it for free.
    /// </summary>
    public sealed class ShotCooldown
    {
        private float _nextShotTime;

        /// <exception cref="ArgumentOutOfRangeException"><paramref name="interval"/> is negative or NaN.</exception>
        public ShotCooldown(float interval)
        {
            if (!(interval >= 0f))
                throw new ArgumentOutOfRangeException(nameof(interval), interval, "The shot interval cannot be negative.");

            Interval = interval;
        }

        /// <summary>Seconds between two shots.</summary>
        public float Interval { get; }

        /// <summary>True when <paramref name="now"/> has reached the end of the last interval; the first shot is always ready.</summary>
        public bool IsReady(float now) => now >= _nextShotTime;

        /// <summary>Starts a new interval at <paramref name="now"/>.</summary>
        public void Consume(float now) => _nextShotTime = now + Interval;
    }
}
