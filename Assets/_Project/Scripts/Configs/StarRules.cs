using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace DreamVille.Configs
{
    /// <summary>Score thresholds a level's stars are awarded at, in ascending order.</summary>
    [CreateAssetMenu(fileName = "StarRules", menuName = "DreamVille/Scoring/Star Rules")]
    public sealed class StarRules : ScriptableObject
    {
        [FormerlySerializedAs("starThresholds")]
        [SerializeField] private float[] _thresholds;

        /// <summary>Total score needed for each star, for example [50, 100, 150].</summary>
        public IReadOnlyList<float> Thresholds => _thresholds;

#if UNITY_INCLUDE_TESTS
        internal static StarRules CreateForTests(params float[] thresholds)
        {
            var rules = CreateInstance<StarRules>();
            rules._thresholds = thresholds;
            return rules;
        }
#endif
    }
}
