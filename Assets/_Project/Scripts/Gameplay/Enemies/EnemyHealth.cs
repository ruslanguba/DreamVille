using System;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

using DreamVille.Audio;
using DreamVille.Gameplay.Damage;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>
    /// Enemy hit-point holder. Delegates bookkeeping to <see cref="Damage.Health"/> so a
    /// second hit or trigger after death cannot re-fire <see cref="OnDeath"/>.
    /// </summary>
    public sealed class EnemyHealth : MonoBehaviour, IDamageable
    {
        /// <summary>Raised once on death. Arguments: this enemy, the ricochet count of the killing hit.</summary>
        public event Action<EnemyHealth, int> OnDeath;

        [FormerlySerializedAs("_health")]
        [SerializeField] private float _maxHealth = 2f;
        [SerializeField] private AudioClip _clip;

        private IAudioService _audio;
        private Health _hitPoints;
        private int _collisionsCount;

        /// <summary>True once this enemy has died; further damage is ignored.</summary>
        public bool IsDead => _hitPoints.IsDead;

        /// <summary>False while a spawn or flee immunity window is open; damage is ignored then.</summary>
        public bool IsVulnerable { get; private set; } = true;

        [Inject]
        public void Construct(IAudioService audio)
        {
            _audio = audio;
        }

        public void SetVulnerable(bool isVulnerable)
        {
            IsVulnerable = isVulnerable;
        }

        public void TakeDamage(float damageValue, int collisionsCount)
        {
            if (IsDead || !IsVulnerable)
                return;

            _collisionsCount = collisionsCount;
            if (_hitPoints.TakeDamage(damageValue))
                Die();
        }

        private void Awake()
        {
            _hitPoints = new Health(_maxHealth);
        }

        private void Die()
        {
            if (_clip != null)
                _audio?.PlaySfx(_clip);

            OnDeath?.Invoke(this, _collisionsCount);
        }

        private void OnDestroy()
        {
            OnDeath = null;
        }
    }
}
