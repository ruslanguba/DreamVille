using System;
using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Aiming
{
    /// <summary>
    /// Pure bounce-path math: a sphere of the bullet's radius, reflected at each contact, within the bullet's
    /// contact and distance budget. The path ends where the bullet would return to its pool.
    /// </summary>
    public static class BouncePath
    {
        /// <summary>Points buffer length that fits any path with <paramref name="maxContacts"/> contacts.</summary>
        public static int RequiredCapacity(int maxContacts) => maxContacts + 1;

        /// <summary>
        /// Writes the path into <paramref name="points"/>: the origin, then the sphere centre at each contact.
        /// When the sphere reaches no surface, the last point is where the remaining length runs out.
        /// The bullet dies on its last contact, so after <paramref name="maxContacts"/> contacts the path stops there.
        /// </summary>
        /// <returns>Number of points written.</returns>
        /// <exception cref="ArgumentException"><paramref name="points"/> is shorter than <see cref="RequiredCapacity"/>.</exception>
        public static int Compute(in ShotRay ray, float radius, int maxContacts, float maxLength, ISphereCaster caster, Vector3[] points)
        {
            if (caster == null)
                throw new ArgumentNullException(nameof(caster));

            if (points == null)
                throw new ArgumentNullException(nameof(points));

            if (maxContacts < 0)
                throw new ArgumentOutOfRangeException(nameof(maxContacts), maxContacts, "The contact count cannot be negative.");

            if (points.Length < RequiredCapacity(maxContacts))
                throw new ArgumentException($"The buffer holds {points.Length} points, {RequiredCapacity(maxContacts)} are required.", nameof(points));

            Vector3 position = ray.Origin;
            Vector3 direction = ray.Direction;
            float remaining = maxLength;

            int count = 0;
            points[count++] = position;

            for (int contact = 0; contact < maxContacts && remaining > 0f; contact++)
            {
                if (!caster.Cast(position, direction, radius, remaining, out float distance, out Vector3 normal))
                {
                    points[count++] = position + direction * remaining;
                    return count;
                }

                // The sphere centre at contact lies on the ray; hit.point would sit on the surface, off the ray for oblique hits.
                position += direction * distance;
                points[count++] = position;
                remaining -= distance;
                direction = Vector3.Reflect(direction, normal);
            }

            return count;
        }
    }
}
