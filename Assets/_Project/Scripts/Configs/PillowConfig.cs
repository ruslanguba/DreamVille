using UnityEngine;

namespace DreamVille.Configs
{
    /// <summary>How the player's pillows are thrown and recalled.</summary>
    [CreateAssetMenu(fileName = "PillowConfig", menuName = "DreamVille/Weapons/Pillow Config")]
    public sealed class PillowConfig : ScriptableObject
    {
        [Tooltip("Impulse applied along the throw direction (N·s).")]
        [SerializeField, Min(0f)] private float _throwImpulse = 10f;

        [Tooltip("Speed of a recalled pillow on its way back (m/s).")]
        [SerializeField, Min(0.01f)] private float _recallSpeed = 12f;

        [Tooltip("A recalled pillow farther than this first jumps onto this radius around the return point (m).")]
        [SerializeField, Min(0f)] private float _recallSnapDistance = 10f;

        [Tooltip("Distance to the return point at which a recalled pillow counts as arrived (m).")]
        [SerializeField, Min(0.001f)] private float _arrivalThreshold = 0.1f;

        /// <summary>Impulse applied along the throw direction.</summary>
        public float ThrowImpulse => _throwImpulse;

        /// <summary>Speed of a recalled pillow in metres per second.</summary>
        public float RecallSpeed => _recallSpeed;

        /// <summary>A recalled pillow farther than this first jumps onto this radius around the return point.</summary>
        public float RecallSnapDistance => _recallSnapDistance;

        /// <summary>Distance to the return point at which a recalled pillow counts as arrived.</summary>
        public float ArrivalThreshold => _arrivalThreshold;

        /// <summary>
        /// False with a reason when a value would stall the recall loop (speed or threshold not positive, or NaN)
        /// or misplace a pillow (negative impulse or snap distance). <c>[Min]</c> only guards the Inspector.
        /// </summary>
        public bool IsValid(out string error)
        {
            // Written as !(x > 0) so NaN fails too.
            if (!(_recallSpeed > 0f))
            {
                error = $"{nameof(RecallSpeed)} must be positive, but is {_recallSpeed}.";
                return false;
            }

            if (!(_arrivalThreshold > 0f))
            {
                error = $"{nameof(ArrivalThreshold)} must be positive, but is {_arrivalThreshold}.";
                return false;
            }

            if (!(_recallSnapDistance >= 0f))
            {
                error = $"{nameof(RecallSnapDistance)} cannot be negative, but is {_recallSnapDistance}.";
                return false;
            }

            if (!(_throwImpulse >= 0f))
            {
                error = $"{nameof(ThrowImpulse)} cannot be negative, but is {_throwImpulse}.";
                return false;
            }

            error = null;
            return true;
        }

#if UNITY_INCLUDE_TESTS
        internal static PillowConfig CreateForTests(
            float throwImpulse = 10f,
            float recallSpeed = 12f,
            float recallSnapDistance = 10f,
            float arrivalThreshold = 0.1f)
        {
            var config = CreateInstance<PillowConfig>();
            config._throwImpulse = throwImpulse;
            config._recallSpeed = recallSpeed;
            config._recallSnapDistance = recallSnapDistance;
            config._arrivalThreshold = arrivalThreshold;
            return config;
        }
#endif
    }
}
