using UnityEngine;

namespace DreamVille.Gameplay.Enemies.Combat
{
    /// <summary>Spawn parameters for a pooled enemy projectile.</summary>
    public readonly struct EnemyProjectileLaunch
    {
        public readonly Vector3 Position;
        public readonly Vector3 Direction;
        public readonly float Impulse;
        public readonly float Lifetime;

        public EnemyProjectileLaunch(Vector3 position, Vector3 direction, float impulse, float lifetime)
        {
            Position = position;
            Direction = direction;
            Impulse = impulse;
            Lifetime = lifetime;
        }
    }
}
