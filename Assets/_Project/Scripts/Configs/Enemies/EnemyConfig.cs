using UnityEngine;

namespace DreamVille.Configs.Enemies
{
    /// <summary>Archetype tunables shared by every enemy instance of one kind.</summary>
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "DreamVille/Enemies/Enemy Config")]
    public sealed class EnemyConfig : ScriptableObject
    {
        [Header("Behaviour")]
        [SerializeField] private EnemyStateId _initialState = EnemyStateId.Patrol;
        [SerializeField] private PatrolMode _patrolMode = PatrolMode.Loop;

        [Header("Tuning")]
        [SerializeField, Min(0f), Tooltip("Metres per second on a route, in a chase and when fleeing. 0 = stationary: no patrol route allowed.")]
        private float _moveSpeed = 3f;

        [SerializeField, Min(0f), Tooltip("Chases inside this distance. 0 = never chases; otherwise greater than Attack Range.")]
        private float _chaseRange = 7f;

        [SerializeField, Min(0f), Tooltip("Stops and throws inside this distance.")]
        private float _attackRange = 5f;

        [SerializeField, Min(0f), Tooltip("Seconds between throws; the first throw is immediate.")]
        private float _attackCooldown = 2f;

        [SerializeField, Min(0f), Tooltip("Facing slerp rate per second. Throwers need > 0: 0 snaps when an attack starts and never tracks.")]
        private float _rotationSpeed = 8f;

        [Header("Steering")]
        [SerializeField, Min(0f)] private float _acceleration = 6f;
        [SerializeField, Min(0f)] private float _patrolArriveRadius = 0.5f;
        [SerializeField, Min(0f)] private float _hideoutArriveRadius = 2f;

        [Header("Attack")]
        [SerializeField] private EnemyAttackKind _attack = EnemyAttackKind.ThrowProjectile;
        [SerializeField, Min(0f)] private float _throwImpulse = 5f;
        [SerializeField, Min(0f)] private float _projectileLifetime = 5f;

        [Header("Survivability")]
        [SerializeField] private bool _spawnsVulnerable = true;
        [SerializeField, Min(0f)] private float _hitImmunityDuration;
        [SerializeField, Min(0f)] private float _despawnDelay = 7f;

        [Header("Presentation")]
        [SerializeField, Range(0f, 1f)] private float _maxLocomotionStartOffset = 0.5f;

        /// <summary>State the machine starts in.</summary>
        public EnemyStateId InitialState => _initialState;

        public PatrolMode PatrolMode => _patrolMode;

        /// <summary>Top speed on a route, in a chase and when fleeing; 0 for an enemy that never moves.</summary>
        public float MoveSpeed => _moveSpeed;

        /// <summary>Distance inside which the enemy chases the player; 0 means it never chases.</summary>
        public float ChaseRange => _chaseRange;

        /// <summary>Distance inside which the enemy stops and throws.</summary>
        public float AttackRange => _attackRange;

        /// <summary>Seconds between throws; the first throw of an attack is immediate.</summary>
        public float AttackCooldown => _attackCooldown;

        /// <summary>Facing slerp rate per second.</summary>
        public float RotationSpeed => _rotationSpeed;

        /// <summary>Steering acceleration applied while patrolling, chasing or fleeing.</summary>
        public float Acceleration => _acceleration;

        /// <summary>Distance to a patrol point at which the enemy considers it reached.</summary>
        public float PatrolArriveRadius => _patrolArriveRadius;

        /// <summary>Distance to the flee target at which the enemy considers a hideout reached.</summary>
        public float HideoutArriveRadius => _hideoutArriveRadius;

        public EnemyAttackKind Attack => _attack;

        /// <summary>Impulse applied to a thrown projectile.</summary>
        public float ThrowImpulse => _throwImpulse;

        /// <summary>Seconds a thrown projectile lives before it despawns on its own.</summary>
        public float ProjectileLifetime => _projectileLifetime;

        /// <summary>False for enemies that spawn with a brief hit-immunity window.</summary>
        public bool SpawnsVulnerable => _spawnsVulnerable;

        /// <summary>Immunity window applied when <see cref="SpawnsVulnerable"/> is false.</summary>
        public float HitImmunityDuration => _hitImmunityDuration;

        /// <summary>Seconds between death and the enemy's GameObject being despawned.</summary>
        public float DespawnDelay => _despawnDelay;

        /// <summary>Upper bound of the random normalized offset the locomotion clip starts at.</summary>
        public float MaxLocomotionStartOffset => _maxLocomotionStartOffset;

#if UNITY_INCLUDE_TESTS
        internal static EnemyConfig CreateForTests(
            EnemyStateId initialState = EnemyStateId.Patrol,
            PatrolMode patrolMode = PatrolMode.Loop,
            float acceleration = 6f,
            float patrolArriveRadius = 0.5f,
            float hideoutArriveRadius = 2f,
            EnemyAttackKind attack = EnemyAttackKind.ThrowProjectile,
            float throwImpulse = 5f,
            float projectileLifetime = 5f,
            bool spawnsVulnerable = true,
            float hitImmunityDuration = 0f,
            float despawnDelay = 7f,
            float maxLocomotionStartOffset = 0.5f,
            float moveSpeed = 3f,
            float chaseRange = 7f,
            float attackRange = 5f,
            float attackCooldown = 2f,
            float rotationSpeed = 8f)
        {
            var config = CreateInstance<EnemyConfig>();
            config._initialState = initialState;
            config._patrolMode = patrolMode;
            config._acceleration = acceleration;
            config._patrolArriveRadius = patrolArriveRadius;
            config._hideoutArriveRadius = hideoutArriveRadius;
            config._attack = attack;
            config._throwImpulse = throwImpulse;
            config._projectileLifetime = projectileLifetime;
            config._spawnsVulnerable = spawnsVulnerable;
            config._hitImmunityDuration = hitImmunityDuration;
            config._despawnDelay = despawnDelay;
            config._maxLocomotionStartOffset = maxLocomotionStartOffset;
            config._moveSpeed = moveSpeed;
            config._chaseRange = chaseRange;
            config._attackRange = attackRange;
            config._attackCooldown = attackCooldown;
            config._rotationSpeed = rotationSpeed;
            return config;
        }
#endif
    }
}
