using System;
using UnityEngine;
using Zenject;

using DreamVille.Audio;
using DreamVille.Core.Events;
using DreamVille.Gameplay.Damage;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>
    /// Enemy hit-point holder. Delegates bookkeeping to <see cref="Damage.Health"/> so a
    /// second hit or trigger after death cannot re-fire <see cref="Killed"/> or <see cref="Died"/>.
    /// </summary>
    public sealed class EnemyHealth : MonoBehaviour, IDamageable, IMortal
    {
        /// <summary>Raised once on death, before <see cref="Died"/>. Arguments: this enemy, the hit count of the killing hit.</summary>
        public event Action<EnemyHealth, int> Killed;

        /// <summary>Raised once on death, right after <see cref="Killed"/>. Payload-free seam for objectives.</summary>
        public event Action<IMortal> Died;

        [SerializeField] private float _maxHealth = 2f;
        [SerializeField] private AudioClip _clip;

        private IAudioService _audio;
        private Health _hitPoints;
        private int _hitCount;

        /// <summary>
        /// True once this enemy has died; further damage is ignored. An enemy that has not woken up yet
        /// (inactive, waiting for its wave) is alive.
        /// </summary>
        public bool IsDead => _hitPoints != null && _hitPoints.IsDead;

        /// <summary>False while a spawn or flee immunity window is open; damage is ignored then.</summary>
        public bool IsVulnerable { get; private set; } = true;

        [Inject]
        private void Construct(IAudioService audio)
        {
            _audio = audio;
        }

        public void SetVulnerable(bool isVulnerable)
        {
            IsVulnerable = isVulnerable;
        }

        public void TakeDamage(float damageValue, int hitCount)
        {
            if (IsDead || !IsVulnerable)
                return;

            _hitCount = hitCount;
            if (HitPoints.TakeDamage(damageValue))
                Die();
        }

        /// <summary>Kills regardless of the immunity window (fall volumes). Does nothing once dead.</summary>
        public void Kill(int hitCount)
        {
            if (IsDead)
                return;

            _hitCount = hitCount;
            if (HitPoints.TakeDamage(HitPoints.Current))
                Die();
        }

        // Built on first damage rather than in Awake: an enemy damaged before it woke up still dies, and EditMode
        // tests can drive the component. Health is a plain class, so ??= is safe here.
        private Health HitPoints => _hitPoints ??= new Health(_maxHealth);

        private void Die()
        {
            PlayDeathClip();

            // Isolated: one throwing listener must not keep the registry, the objective or the state machine from
            // hearing about the death, or the level may never complete.
            IsolatedEvent.Raise(Killed, this, _hitCount, this);
            IsolatedEvent.Raise<IMortal>(Died, this, this);
        }

        private void PlayDeathClip()
        {
            if (_clip == null)
                return;

            if (_audio == null)
            {
                Debug.LogError($"[{nameof(EnemyHealth)}] {nameof(IAudioService)} is not bound; the death clip stays silent.", this);
                return;
            }

            _audio.PlaySfx(_clip);
        }

        private void OnDestroy()
        {
            Killed = null;
            Died = null;
        }
    }
}
