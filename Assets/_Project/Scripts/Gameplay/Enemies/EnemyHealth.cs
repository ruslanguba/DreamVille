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
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        /// <summary>Raised once on death. Arguments: this enemy, the ricochet count of the killing hit.</summary>
        public event Action<EnemyHealth, int> OnDeath;

        [FormerlySerializedAs("_health")]
        [SerializeField] private float _maxHealth = 2f;
        [SerializeField] private AudioClip _clip;

        protected int _collisionsCount;

        private IAudioService _audio;
        private Health _hitPoints;

        /// <summary>True once this enemy has died; further damage is ignored.</summary>
        public bool IsDead => _hitPoints.IsDead;

        [Inject]
        public void Construct(IAudioService audio)
        {
            _audio = audio;
        }

        /// <summary>Subclasses that declare their own Awake must call <c>base.Awake()</c>.</summary>
        protected virtual void Awake()
        {
            _hitPoints = new Health(_maxHealth);
        }

        public virtual void TakeDamage(float damageValue, int collisionsCount)
        {
            if (IsDead)
                return;

            _collisionsCount = collisionsCount;
            if (_hitPoints.TakeDamage(damageValue))
                Die();
        }

        protected virtual void Die()
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
