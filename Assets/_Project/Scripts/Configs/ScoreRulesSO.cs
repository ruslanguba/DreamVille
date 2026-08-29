using UnityEngine;

namespace DreamVille.Configs
{
    [CreateAssetMenu(fileName = "ScoreRulesSO", menuName = "Scriptable Objects/ScoreRulesSO")]
    public class ScoreRulesSO : ScriptableObject
    {
        [Header("Base")]
        public float scorePerKill = 10f;

        [Header("Ricochet bonus")]
        public bool useRicochetMultiplier = true;
        public float ricochetMultiplier = 1f;
    }
}
