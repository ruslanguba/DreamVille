using UnityEngine;

namespace DreamVille.Configs
{
    /// <summary>How a kill converts into score.</summary>
    [CreateAssetMenu(fileName = "ScoreRules", menuName = "DreamVille/Scoring/Score Rules")]
    public sealed class ScoreRules : ScriptableObject
    {
        [SerializeField, Min(0f)] private float _scorePerKill = 10f;

        /// <summary>Score granted per killed enemy, before the hit-count multiplier.</summary>
        public float ScorePerKill => _scorePerKill;

#if UNITY_INCLUDE_TESTS
        internal static ScoreRules CreateForTests(float scorePerKill)
        {
            var rules = CreateInstance<ScoreRules>();
            rules._scorePerKill = scorePerKill;
            return rules;
        }
#endif
    }
}
