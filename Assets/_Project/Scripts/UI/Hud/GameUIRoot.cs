using UnityEngine;

namespace DreamVille.UI.Hud
{
    /// <summary>
    /// Root of the gameplay HUD prefab. Exposes its five blocks to the presenter through the view interfaces
    /// and holds no behaviour of its own.
    /// </summary>
    public sealed class GameUIRoot : MonoBehaviour, IGameHudView
    {
        [Header("HUD Blocks")]
        [SerializeField] private RicochetView _ricochet;
        [SerializeField] private PillowView _pillows;
        [SerializeField] private EnemyCounterView _enemyCounter;
        [SerializeField] private ScorePopupView _scorePopup;
        [SerializeField] private StarsView _stars;

        public IScoreView Score => _scorePopup;
        public IStarsView Stars => _stars;
        public IPillowView Pillows => _pillows;
        public IEnemyCounterView EnemyCounter => _enemyCounter;
        public IRicochetView Ricochet => _ricochet;

        private void Awake()
        {
            ReportIfMissing(_ricochet, nameof(_ricochet));
            ReportIfMissing(_pillows, nameof(_pillows));
            ReportIfMissing(_enemyCounter, nameof(_enemyCounter));
            ReportIfMissing(_scorePopup, nameof(_scorePopup));
            ReportIfMissing(_stars, nameof(_stars));
        }

        private void ReportIfMissing(Object block, string fieldName)
        {
            if (block == null)
                Debug.LogError($"[{nameof(GameUIRoot)}] '{name}' has no HUD block assigned to {fieldName}.", this);
        }
    }
}
