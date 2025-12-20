using UnityEngine;

public class GameplaySceneEntryPoint : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameUIRoot _uiPrefab;

    [Header("Systems")]
    [SerializeField] private LevelProgressSystem _levelProgress;
    [SerializeField] private ScoreManager _scoreManager;
    [SerializeField] private BulletEventRelay _bulletRelay;
    [SerializeField] private PillowThrowHandler _pillowHandler;

    private void Awake()
    {
        // UI
        var ui = Instantiate(_uiPrefab);

        // UI bindings
        ui.BindScore(_scoreManager);
        ui.BindRicochets(_bulletRelay);
        ui.BindPillows(_pillowHandler);
    }
}
