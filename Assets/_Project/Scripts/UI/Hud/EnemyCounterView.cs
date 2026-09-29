using TMPro;
using UnityEngine;

namespace DreamVille.UI.Hud
{
    /// <summary>Text with the number of enemies still alive.</summary>
    public sealed class EnemyCounterView : MonoBehaviour, IEnemyCounterView
    {
        [SerializeField] private TextMeshProUGUI _enemiesToKill;

        public void SetRemaining(int remaining)
        {
            _enemiesToKill.text = remaining.ToString();
        }
    }
}
