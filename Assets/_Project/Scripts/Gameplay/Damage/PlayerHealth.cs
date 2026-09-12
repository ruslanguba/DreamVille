using System;

using UnityEngine;
using Zenject;

using DreamVille.Configs;

namespace DreamVille.Gameplay.Damage
{
    /// <summary>Recovering hit counter for the player. The game has no death, only hits and recovery over time.</summary>
    public sealed class PlayerHealth : ITickable
    {
        public event Action<int> HitsChanged;
        public event Action Recovered;

        private readonly PlayerConfig _config;
        private int _hits;
        private float _recoveryTimer;

        public PlayerHealth(PlayerConfig config)
        {
            _config = config;
        }

        public int Hits => _hits;
        public int MaxHits => _config.MaxHits;
        public bool IsHurt => _hits > 0;

        /// <summary>Registers a hit, clamps at <see cref="MaxHits"/> and restarts the recovery timer.</summary>
        public void RegisterHit()
        {
            _hits = Mathf.Min(_hits + 1, _config.MaxHits);
            _recoveryTimer = _config.HitRecoveryDelay;
            HitsChanged?.Invoke(_hits);
        }

        /// <summary>Counts down the recovery timer while hurt; resets the hit count to zero once it elapses.</summary>
        public void Tick(float deltaTime)
        {
            if (_hits <= 0)
                return;

            _recoveryTimer -= deltaTime;
            if (_recoveryTimer > 0f)
                return;

            _hits = 0;
            _recoveryTimer = 0f;
            Recovered?.Invoke();
        }

        void ITickable.Tick() => Tick(Time.deltaTime);
    }
}
