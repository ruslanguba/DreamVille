using UnityEngine;

using DreamVille.UI;

namespace DreamVille.Core.Bootstrap
{
    public class GameUIRoot : MonoBehaviour
    {
        [Header("UI Blocks")]
        [SerializeField] private RicochetUI _ricochetUI;
        [SerializeField] private PillowUI _pillowUI;
        [SerializeField] private EnemyCounterUI _enemyCounterUI;
        [SerializeField] private ScorePopupUI _scorePopupUI;
        [SerializeField] private StarsUI _starsUI;

        // Публичные ссылки для централизованных подписок
        public RicochetUI RicochetUI => _ricochetUI;
        public PillowUI PillowUI => _pillowUI;
        public EnemyCounterUI EnemyCounter => _enemyCounterUI;
        public ScorePopupUI ScorePopup => _scorePopupUI;
        public StarsUI StarsUI => _starsUI;
    }
}
