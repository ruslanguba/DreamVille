using System.Collections.Generic;
using UnityEngine;

using DreamVille.Gameplay.Weapons.Aiming;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Hand-written double for <see cref="ISphereCaster"/>: a world of infinite planes. A cast reports the nearest
    /// plane the sphere approaches and touches within the distance budget, and records the radius it was asked for.
    /// </summary>
    public sealed class FakeSphereCaster : ISphereCaster
    {
        private const float MinDistance = 1e-4f;

        private readonly List<(Vector3 Point, Vector3 Normal)> _planes = new List<(Vector3, Vector3)>();

        public float LastRadius { get; private set; }

        public int CastCount { get; private set; }

        /// <summary>Adds a plane through <paramref name="point"/> whose solid side is behind <paramref name="normal"/>.</summary>
        public void AddPlane(Vector3 point, Vector3 normal) => _planes.Add((point, normal.normalized));

        public bool Cast(Vector3 origin, Vector3 direction, float radius, float maxDistance, out float distance, out Vector3 normal)
        {
            CastCount++;
            LastRadius = radius;

            distance = float.PositiveInfinity;
            normal = Vector3.zero;

            foreach ((Vector3 point, Vector3 planeNormal) in _planes)
            {
                float approach = Vector3.Dot(direction, planeNormal);
                if (approach >= 0f)
                    continue;

                // The sphere centre touches the plane when its signed distance to it equals the radius.
                float signedDistance = Vector3.Dot(origin - point, planeNormal);
                float travel = (radius - signedDistance) / approach;

                if (travel > MinDistance && travel <= maxDistance && travel < distance)
                {
                    distance = travel;
                    normal = planeNormal;
                }
            }

            if (float.IsPositiveInfinity(distance))
            {
                distance = 0f;
                return false;
            }

            return true;
        }
    }
}
