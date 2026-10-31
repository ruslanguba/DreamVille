using UnityEngine;

namespace DreamVille.Gameplay.Enemies.Combat
{
    /// <summary>Spawn parameters for a pooled enemy projectile.</summary>
    public readonly struct EnemyProjectileLaunch
    {
        public EnemyProjectileLaunch(Vector3 position, Vector3 direction, float impulse, float lifetime)
        {
            Position = position;
            Direction = direction;
            Impulse = impulse;
            Lifetime = lifetime;
        }

        public Vector3 Position { get; }

        public Vector3 Direction { get; }

        public float Impulse { get; }

        public float Lifetime { get; }
    }
}
