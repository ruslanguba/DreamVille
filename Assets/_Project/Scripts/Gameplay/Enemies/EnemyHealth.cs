using System;
using UnityEngine.Events;
using UnityEngine;
using Zenject;

using DreamVille.Audio;
using DreamVille.Gameplay.Damage;

namespace DreamVille.Gameplay.Enemies
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        public event Action<EnemyHealth, int> OnDeath;
        [SerializeField] private float _health = 2f;
        [SerializeField] private AudioClip _clip;
        //[SerializeField] private ParticleSystem _particleSystem;
        protected int _collisionsCount;

        private AudioManager _audio;

        [Inject]
        public void Construct(AudioManager audio)
        {
            _audio = audio;
        }

        public virtual void TakeDamage(float damageValue, int collisionsCount)
        {
            _health -= damageValue;
            _collisionsCount = collisionsCount;
            if (_health <= 0)
            {
                //_particleSystem.Play();
                Die();
            }
        }

        //private void PlayParticle()
        //{
        //    _particleSystem.transform.position = point;
        //    _particle.Play();
        //}

        protected virtual void Die()
        {
            if (_clip != null)
            {
                _audio?.PlaySFX(_clip);
            }
            OnDeath?.Invoke(this, _collisionsCount);
        }

        private void OnDestroy()
        {
            OnDeath = null;
        }
    }
}
