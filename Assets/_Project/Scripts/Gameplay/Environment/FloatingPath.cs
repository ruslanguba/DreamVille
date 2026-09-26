using UnityEngine;

namespace DreamVille.Gameplay.Environment
{
    /// <summary>Target picking for the floating props: origin + offset with the height clamped.</summary>
    public static class FloatingPath
    {
        /// <summary>
        /// <paramref name="origin"/> + <paramref name="offset"/>, with y clamped to
        /// [<paramref name="minY"/>, <paramref name="maxY"/>] so a prop never drifts out of the play space.
        /// </summary>
        public static Vector3 PickTarget(Vector3 origin, Vector3 offset, float minY, float maxY)
        {
            Vector3 target = origin + offset;
            target.y = Mathf.Clamp(target.y, minY, maxY);
            return target;
        }
    }
}
