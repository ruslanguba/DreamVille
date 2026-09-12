using UnityEngine;
using UnityEngine.Serialization;

using DreamVille.Gameplay.Enemies.StateMachine;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>
    /// Enemy variant with a brief post-spawn hit immunity window and a ragdoll-ish death.
    /// </summary>
    public class SimpleDie : EnemyHealth
    {
        private Rigidbody _rigidbody;
        private Animator _animator;
        [SerializeField] private float _timeToEnableHit = 0.5f;
        private float _timer;
        [FormerlySerializedAs("_isCanTakeHit")]
        [SerializeField] private bool _canTakeHit;
        [SerializeField] private SimpleRunEnemy _simpleRunEnemy;
        public bool IsAlive { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            IsAlive = true;
        }

        private void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            _timer = _timeToEnableHit;
            _canTakeHit = false;
        }

        public override void TakeDamage(float damageValue, int collisions)
        {
            if (_canTakeHit)
            {
                base.TakeDamage(damageValue, collisions);
            }
        }

        private void Update()
        {
            if (_timer > 0)
            {
                _timer -= Time.deltaTime;
                if (_timer <= 0)
                {
                    _canTakeHit = true;
                }
            }
        }

        /// <summary>Sets whether this enemy can currently take damage and resets the immunity timer.</summary>
        public void SetVulnerable(bool isVulnerable)
        {
            _canTakeHit = isVulnerable;
            _timer = _timeToEnableHit;
        }

        protected override void Die()
        {
            if (!IsAlive)
                return;

            IsAlive = false;
            GetComponent<EnemyRotationHandler>().enabled = false;
            _simpleRunEnemy.StopMoving();

            _animator.SetTrigger("dead");
            _rigidbody.freezeRotation = false;
            _rigidbody.useGravity = true;

            base.Die();
            Destroy(gameObject, 7f);
        }
    }
}
