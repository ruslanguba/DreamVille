using UnityEngine;

namespace DreamVille.Configs
{
    [CreateAssetMenu(fileName = "StarRulesSO", menuName = "Scriptable Objects/StarRulesSO")]
    public class StarRulesSO : ScriptableObject
    {
        public string levelId;
        public float[] starThresholds; // например: [50, 100, 150]
    }
}
