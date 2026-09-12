using System;
using UnityEngine;

namespace DreamVille.Gameplay.Enemies
{
    /// <summary>
    /// Route hygiene for designer-authored patrol arrays. Shipping scenes contain routes with
    /// empty slots (a sized array nobody filled); an empty slot is not a point, so it is dropped
    /// and a route made only of empty slots behaves exactly like no route at all.
    /// </summary>
    public static class PatrolRoute
    {
        /// <summary>Returns <paramref name="points"/> itself when it has no empty slots, otherwise a compacted copy.</summary>
        public static Transform[] WithoutEmptySlots(Transform[] points)
        {
            if (points == null)
                return Array.Empty<Transform>();

            int count = 0;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] != null)
                    count++;
            }

            if (count == points.Length)
                return points;

            var route = new Transform[count];
            int next = 0;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] != null)
                    route[next++] = points[i];
            }

            return route;
        }
    }
}
