using UnityEngine;

public class GameplaySceneEntryPoint : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameUIRoot _uiPrefab;
    [SerializeField] private PlayerContext _playerPrefab;

    [Header("Config")]
    [SerializeField] private ScoreRulesSO _scoreRules;
    [SerializeField] private StarRulesSO _starRules;

    [Header("Systems")]
    [SerializeField] private BulletEventRelay _bulletRelay;
    [SerializeField] private PillowThrowHandler _pillowHandler;
    [SerializeField] private EnemyRegistry _enemyRegistry;
    [SerializeField] private LevelCompletionFlow _levelCompleatFlow;

    private IPlayerInput _input;
    private ScoreService _scoreService;
    private StarProgressService _starProgressService;
    private GameUIRoot _ui;

    private void Awake()
    {
        _scoreService = new ScoreService(_scoreRules);
        _starProgressService = new StarProgressService(_starRules);

        _ui = Instantiate(_uiPrefab);
        BindUI();
        BindEvents();

        InitializePillowThrowHandler();
        SetupEnemyRegistry();
    }

    private void BindUI()
    {
        // Подписка UI на сервисы
        _scoreService.OnScoreAdded += _ui.ScorePopup.ShowScore;
        _starProgressService.OnStarProgressChanged += _ui.StarsUI.SetStarProgress;
        _starProgressService.OnStarUnlocked += _ui.StarsUI.UnlockStar;

        _pillowHandler.OnPillowAdded += _ui.PillowUI.SpawnIcons;
        _pillowHandler.OnThrow += _ui.PillowUI.HidePillow;
        _pillowHandler.OnRecall += _ui.PillowUI.ShowPillow;

        _bulletRelay.BulletHit += _ui.RicochetUI.OnBulletHit;
    }

    private void BindEvents()
    {
        _scoreService.OnScoreAdded += _starProgressService.AddScore;
        _enemyRegistry.OnAllEnemiesKilled += _levelCompleatFlow.LevelCompleat;

    }

    private void SetupEnemyRegistry()
    {
        _enemyRegistry.Initialize();
        _ui.EnemyCounter.SetEnemiesRemaining(_enemyRegistry.TotalEnemies);

        _enemyRegistry.OnEnemyKilled += OnEnemyKilled;
    }

    private void OnEnemyKilled(int remainingEnemies, int collisionsCount)
    {
        _scoreService.HandleEnemyKilled(collisionsCount);
        _ui.EnemyCounter.SetEnemiesRemaining(remainingEnemies);
    }

    private void InitializePillowThrowHandler()
    {
        _pillowHandler.Initialize();
    }
}
