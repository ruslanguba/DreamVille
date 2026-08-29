using TMPro;
using UnityEngine;

namespace DreamVille.UI
{
    public class EnemyCounterUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _enemiesToKill;

        public void SetEnemiesRemaining(int remaining)
        {
            _enemiesToKill.text = remaining.ToString();
        }
    }
}
