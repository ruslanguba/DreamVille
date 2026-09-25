using UnityEngine;
using UnityEngine.Serialization;

namespace DreamVille.Configs
{
    /// <summary>Flight and damage of a projectile. The aim preview reads the same values, so it ends where the bullet dies.</summary>
    [CreateAssetMenu(fileName = "BulletConfig", menuName = "DreamVille/Weapons/Bullet Config")]
    public sealed class BulletConfig : ScriptableObject
    {
        [FormerlySerializedAs("Damage")]
        [SerializeField, Min(0f)] private float _damage = 10f;

        [FormerlySerializedAs("Speed")]
        [SerializeField, Min(0.01f)] private float _speed = 15f;

        [FormerlySerializedAs("LifeTime")]
        [SerializeField, Min(0.01f)] private float _lifeTime = 8f;

        [FormerlySerializedAs("MaxCollisionCount")]
        [SerializeField, Min(1)] private int _maxCollisionCount = 6;

        [SerializeField, Min(0.01f)] private float _collisionRadius = 0.25f;

        public float Damage => _damage;

        /// <summary>Flight speed in metres per second.</summary>
        public float Speed => _speed;

        /// <summary>Seconds the bullet flies before it returns to the pool.</summary>
        public float LifeTime => _lifeTime;

        /// <summary>Contacts before the bullet returns to the pool (it dies on this one).</summary>
        public int MaxCollisionCount => _maxCollisionCount;

        /// <summary>Radius of the bullet's sphere collider; the aim preview sweeps the same sphere.</summary>
        public float CollisionRadius => _collisionRadius;

        /// <summary>Distance the bullet can travel before its lifetime ends (Speed × LifeTime).</summary>
        public float MaxTravelDistance => _speed * _lifeTime;

#if UNITY_INCLUDE_TESTS
        internal static BulletConfig CreateForTests(
            float damage = 10f,
            float speed = 15f,
            float lifeTime = 8f,
            int maxCollisionCount = 6,
            float collisionRadius = 0.25f)
        {
            var config = CreateInstance<BulletConfig>();
            config._damage = damage;
            config._speed = speed;
            config._lifeTime = lifeTime;
            config._maxCollisionCount = maxCollisionCount;
            config._collisionRadius = collisionRadius;
            return config;
        }
#endif
    }
}
