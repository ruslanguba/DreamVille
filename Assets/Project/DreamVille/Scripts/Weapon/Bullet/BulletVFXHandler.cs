using UnityEngine;

public class BulletVFXHandler : MonoBehaviour
{
    private ParticleSystem _effect;
    private Bullet _bullet;

    private void Awake()
    {
        _bullet = GetComponent<Bullet>();
    }

    private void OnEnable()
    {
        _bullet.Hit += OnHit;
    }

    private void OnDisable()
    {
        _bullet.Hit -= OnHit;
    }

    public void Init(BulletConfig config)
    {
        _effect = config.HitEffect;
    }

    private void OnHit(BulletHitInfo info)
    {
        if (_effect == null) return;
        _effect.transform.position = info.HitPoint;
        _effect.Play();
    }
}
