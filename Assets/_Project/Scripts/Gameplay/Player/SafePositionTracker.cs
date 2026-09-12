using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Gameplay.Player
{
    /// <summary>Remembers the last position the player stood on safe ground, sampled at a fixed interval.</summary>
    public sealed class SafePositionTracker
    {
        private readonly PlayerConfig _config;
        private Vector3 _lastSafePosition;
        private float _timer;

        public SafePositionTracker(PlayerConfig config)
        {
            _config = config;
        }

        public Vector3 LastSafePosition => _lastSafePosition;

        /// <summary>Sets the remembered position and restarts the sampling interval so the next safe frame saves immediately.</summary>
        public void Reset(Vector3 position)
        {
            _lastSafePosition = position;
            _timer = 0f;
        }

        /// <summary>Timer runs only while <paramref name="isSafeGround"/> is true; starts at 0 so the first safe frame saves.</summary>
        public void Tick(float deltaTime, bool isSafeGround, Vector3 position)
        {
            if (!isSafeGround)
                return;

            _timer -= deltaTime;
            if (_timer > 0f)
                return;

            _lastSafePosition = position;
            _timer = _config.SafePositionInterval;
        }
    }
}
