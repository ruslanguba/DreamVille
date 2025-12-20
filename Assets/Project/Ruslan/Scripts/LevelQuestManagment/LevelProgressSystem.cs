using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelProgressSystem : MonoBehaviour
{
    
    public event Action<int> OnEnemeyKilled;
    public event Action OnLevelCompleat;
    [SerializeField] private Canvas _scoreCanvasPrefab;
    [SerializeField] private List<EnemyHealth> enemiesToKill;
    [SerializeField] private float _distance = 8;

    [SerializeField] private string _levelId;
    [SerializeField] private float _screToFillFirstStar;
    [SerializeField] private float _screToFillSecondStar;
    [SerializeField] private float _screToFillThirdStar;

    private ScoreUI _scoreUI;
    private RicochetUI _ricochetUI;
    private PillowUI _pilllowUI;

    private EnemyKillObserver _enemyHandler;
    private ScoreManager _scoreManager;
    private PillowUiBinder _pilllowUIbinder;
    private BulletEventRelay _bulletEventRelay;

    int enemiesCount;

    private void Awake()
    {
        //var canvas = Instantiate(_scoreCanvasPrefab);
        //_scoreUI = canvas.GetComponent<ScoreUI>();
        //_ricochetUI = canvas.GetComponent<RicochetUI>();
        //_pilllowUI = canvas.GetComponent<PillowUI>();

        //_scoreManager = GetComponent<ScoreManager>();
        //_enemyHandler = GetComponent<EnemyKillObserver>();
        //_pilllowUIbinder = GetComponent<PillowUiBinder>();
        //_bulletEventRelay = GetComponent<BulletEventRelay>();

        //_scoreUI.SetScoreManager(_scoreManager);
        _ricochetUI.Bind(_bulletEventRelay);
        //_pilllowUIbinder.SetUI(_pilllowUI);
        FindAllEnemies();
    }

    private void Start()
    {
        enemiesCount = enemiesToKill.Count;
        if (_scoreUI == null)
        {
            _scoreUI = FindFirstObjectByType<ScoreUI>();
        }
        _scoreUI.UpdateEnemiesToKillText(enemiesCount.ToString());
        _scoreUI.SetStarsScore(_levelId, _screToFillFirstStar, _screToFillSecondStar, _screToFillThirdStar);
    }


    private void DetectKill(EnemyHealth enemy, int collisionsCount)
    {
        enemy.OnDeath -= DetectKill;
        enemiesToKill.Remove(enemy);
        CheckIfCompleat();
        _scoreManager.HandleHit(collisionsCount);
        _scoreUI.UpdateEnemiesToKillText(enemiesToKill.Count.ToString());
    }

    private void CheckIfCompleat()
    {
        if (enemiesToKill.Count <= 0)
        {
            Debug.Log("LevelCompleat");
            //CompleatLevel();
            OnLevelCompleat?.Invoke();
        }
    }

    private void FindAllEnemies()
    {
        foreach (EnemyHealth health in FindObjectsByType<EnemyHealth>(0))
        {
            enemiesToKill.Add(health);
            health.OnDeath += DetectKill;
        }
    }
}

//{
//    public event Action<int> OnEnemeyKilled;
//    public event Action OnLevelCompleat;
//    [SerializeField] private Canvas _scoreCanvasPrefab;
//    [SerializeField] private List<EnemyHealth> enemiesToKill;
//    [SerializeField] private float _distance = 8;

//    [SerializeField] private string _levelId;
//    [SerializeField] private float _screToFillFirstStar;
//    [SerializeField] private float _screToFillSecondStar;
//    [SerializeField] private float _screToFillThirdStar;

//    private ScoreUI _scoreUI;
//    private RicochetUI _ricochetUI;
//    private PillowUI _pilllowUI;

//    private EnemyKillObserver _enemyHandler;
//    private ScoreManager _scoreManager;
//    private PillowUiBinder _pilllowUIbinder;
//    private BulletEventRelay _bulletEventRelay;

//    int enemiesCount;

//    private void Awake()
//    {
//        var canvas = Instantiate(_scoreCanvasPrefab);
//        _scoreUI = canvas.GetComponent<ScoreUI>();
//        _ricochetUI = canvas.GetComponent<RicochetUI>();
//        _pilllowUI = canvas.GetComponent<PillowUI>();

//        _scoreManager = GetComponent<ScoreManager>();
//        _enemyHandler = GetComponent<EnemyKillObserver>();
//        _pilllowUIbinder = GetComponent<PillowUiBinder>();
//        _bulletEventRelay = GetComponent<BulletEventRelay>();

//        //_scoreUI.SetScoreManager(_scoreManager);
//        _ricochetUI.Bind(_bulletEventRelay);
//        //_pilllowUIbinder.SetUI(_pilllowUI);
//        FindAllEnemies();
//    }

//    private void Start()
//    {
//        enemiesCount = enemiesToKill.Count;
//        if (_scoreUI == null)
//        {
//            _scoreUI = FindFirstObjectByType<ScoreUI>();
//        }
//        _scoreUI.UpdateEnemiesToKillText(enemiesCount.ToString());
//        _scoreUI.SetStarsScore(_levelId, _screToFillFirstStar, _screToFillSecondStar, _screToFillThirdStar);
//    }


//    private void DetectKill(EnemyHealth enemy, int collisionsCount)
//    {
//        enemy.OnDeath -= DetectKill;
//        enemiesToKill.Remove(enemy);
//        CheckIfCompleat();
//        _scoreManager.HandleHit(collisionsCount);
//        _scoreUI.UpdateEnemiesToKillText(enemiesToKill.Count.ToString());
//    }

//    private void CheckIfCompleat()
//    {
//        if (enemiesToKill.Count <= 0)
//        {
//            Debug.Log("LevelCompleat");
//            //CompleatLevel();
//            OnLevelCompleat?.Invoke();
//        }
//    }

//    private void FindAllEnemies()
//    {
//        foreach(EnemyHealth health in FindObjectsByType<EnemyHealth>(0))
//        {
//            enemiesToKill.Add(health);
//            health.OnDeath += DetectKill;
//        }
//    }
//}
