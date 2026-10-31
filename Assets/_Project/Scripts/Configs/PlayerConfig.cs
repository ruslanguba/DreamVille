using UnityEngine;

namespace DreamVille.Configs
{
    /// <summary>Tunables for the player's movement, jump, knockback, damage and respawn systems.</summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "DreamVille/Player/Player Config")]
    public sealed class PlayerConfig : ScriptableObject
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float _walkSpeed = 5f;
        [SerializeField, Min(0f)] private float _acceleration = 20f;
        [SerializeField, Min(0f)] private float _rotationSpeed = 12f;

        [Header("Jump and gravity")]
        [SerializeField, Min(0f)] private float _jumpHeight = 2f;
        [SerializeField] private float _gravity = -9.81f;
        [SerializeField] private float _groundedStickSpeed = -2f;

        [Header("Knockback")]
        [SerializeField, Min(0f)] private float _knockbackDecay = 10f;
        [SerializeField, Min(0f)] private float _knockbackStopSpeed = 0.1f;

        [Header("Damage")]
        [SerializeField, Min(1)] private int _maxHits = 3;
        [SerializeField, Min(0f)] private float _hitRecoveryDelay = 5f;

        [Header("Respawn")]
        [SerializeField, Min(0f)] private float _safePositionInterval = 0.3f;
        [SerializeField] private float _respawnHeightOffset = 1f;

        public float WalkSpeed => _walkSpeed;
        public float Acceleration => _acceleration;
        public float RotationSpeed => _rotationSpeed;
        public float JumpHeight => _jumpHeight;
        public float Gravity => _gravity;

        /// <summary>Downward speed the vertical velocity is clamped to while grounded and falling.</summary>
        public float GroundedStickSpeed => _groundedStickSpeed;

        /// <summary>Per-second rate the knockback velocity decays toward zero.</summary>
        public float KnockbackDecay => _knockbackDecay;

        /// <summary>Knockback speed below which the residual velocity is zeroed instead of left to linger.</summary>
        public float KnockbackStopSpeed => _knockbackStopSpeed;

        /// <summary>Hit count at which the damage vignette reaches full intensity.</summary>
        public int MaxHits => _maxHits;

        /// <summary>Seconds of no new hits before the hit count starts recovering.</summary>
        public float HitRecoveryDelay => _hitRecoveryDelay;

        /// <summary>Seconds between safe-position samples while the player stands on safe ground.</summary>
        public float SafePositionInterval => _safePositionInterval;

        /// <summary>Vertical offset added above the last safe position on respawn.</summary>
        public float RespawnHeightOffset => _respawnHeightOffset;

#if UNITY_INCLUDE_TESTS
        internal static PlayerConfig CreateForTests(
            float walkSpeed = 5f,
            float acceleration = 20f,
            float rotationSpeed = 12f,
            float jumpHeight = 2f,
            float gravity = -9.81f,
            float groundedStickSpeed = -2f,
            float knockbackDecay = 10f,
            float knockbackStopSpeed = 0.1f,
            int maxHits = 3,
            float hitRecoveryDelay = 5f,
            float safePositionInterval = 0.3f,
            float respawnHeightOffset = 1f)
        {
            var config = CreateInstance<PlayerConfig>();
            config._walkSpeed = walkSpeed;
            config._acceleration = acceleration;
            config._rotationSpeed = rotationSpeed;
            config._jumpHeight = jumpHeight;
            config._gravity = gravity;
            config._groundedStickSpeed = groundedStickSpeed;
            config._knockbackDecay = knockbackDecay;
            config._knockbackStopSpeed = knockbackStopSpeed;
            config._maxHits = maxHits;
            config._hitRecoveryDelay = hitRecoveryDelay;
            config._safePositionInterval = safePositionInterval;
            config._respawnHeightOffset = respawnHeightOffset;
            return config;
        }
#endif
    }
}
