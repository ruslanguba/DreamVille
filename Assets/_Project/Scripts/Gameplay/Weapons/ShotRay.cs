using System;
using UnityEngine;

namespace DreamVille.Gameplay.Weapons
{
    /// <summary>Where a shot leaves and which way it flies. The gun and the aim preview read the same instance.</summary>
    public readonly struct ShotRay
    {
        /// <exception cref="ArgumentException">The direction has no length.</exception>
        public ShotRay(Vector3 origin, Vector3 direction)
        {
            Vector3 normalized = direction.normalized;
            if (normalized == Vector3.zero)
                throw new ArgumentException("A shot needs a non-zero direction.", nameof(direction));

            Origin = origin;
            Direction = normalized;
        }

        public Vector3 Origin { get; }

        /// <summary>Unit-length flight direction.</summary>
        public Vector3 Direction { get; }

        /// <summary>Rotation that looks along <see cref="Direction"/>; the spawned projectile is oriented by it.</summary>
        public Quaternion Rotation => Quaternion.LookRotation(Direction);
    }
}
