using System;

using UnityEngine;
using Zenject;

using DreamVille.Core.Players;

namespace DreamVille.Gameplay.Damage
{
    /// <summary>
    /// The player's hit facade to the scene: registers hits on <see cref="PlayerHealth"/>, pushes the body through
    /// <see cref="PlayerKnockback"/> and exposes the hit state to presentation as <see cref="IPlayerDamageState"/>.
    /// A component because scene-level bindings and trigger callbacks can only find components.
    /// </summary>
    public sealed class PlayerDamageReceiver : MonoBehaviour, IHitReceiver, IPlayerDamageState
    {
        private PlayerHealth _health;
        private PlayerKnockback _knockback;

        [Inject]
        private void Construct(PlayerHealth health, PlayerKnockback knockback)
        {
            _health = health;
            _knockback = knockback;
        }

        public int Hits => _health.Hits;
        public int MaxHits => _health.MaxHits;

        public event Action<int> HitsChanged
        {
            add => _health.HitsChanged += value;
            remove => _health.HitsChanged -= value;
        }

        public event Action Recovered
        {
            add => _health.Recovered += value;
            remove => _health.Recovered -= value;
        }

        public void ReceiveHit(Vector3 direction, float force)
        {
            _health.RegisterHit();
            _knockback.Apply(direction, force);
        }
    }
}
