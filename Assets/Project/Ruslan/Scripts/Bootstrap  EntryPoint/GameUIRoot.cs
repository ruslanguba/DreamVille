using UnityEngine;

public class GameUIRoot : MonoBehaviour
{
    [Header("UI Blocks")]
    [SerializeField] private RicochetUI _ricochetUI;
    [SerializeField] private ScoreUI _scoreUI;
    [SerializeField] private PillowUI _pillowUI;

    public void BindScore(ScoreManager score)
    {
        _scoreUI.Bind(score);
    }

    public void BindRicochets(BulletEventRelay relay)
    {
        _ricochetUI.Bind(relay);
    }

    public void BindPillows(PillowThrowHandler handler)
    {
        _pillowUI.Bind(handler);
    }
}
