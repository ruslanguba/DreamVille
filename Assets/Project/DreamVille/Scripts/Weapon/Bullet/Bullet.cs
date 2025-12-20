using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public event Action<BulletHitInfo> Hit;
    public event Action<Bullet> ReturnedToPool;

    private BulletPool _pool;
    private Rigidbody _rb;
    private int _hitCount;
    private BulletConfig _config;

    private BulletDamageHandler _damageHandler;
    private BulletInteractionHandler _interactionHandler;
    private BulletVFXHandler _vfxHandler;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void SetPool(BulletPool pool)
    {
        _pool = pool;
    }
    public void Init(BulletConfig config)
    {
        _config = config;
        _hitCount = 0;
        CancelInvoke();
        Invoke(nameof(ReturnToPool), _config.LifeTime);
    }
    public void Launch(Vector3 direction)
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.AddForce(direction * _config.Speed, ForceMode.Impulse);
        CancelInvoke();
        Invoke(nameof(ReturnToPool), _config.LifeTime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        _hitCount++;
        Debug.Log("Bullet OnBulletHit");
        Hit?.Invoke(new BulletHitInfo
        {
            HitPoint = collision.contacts[0].point,
            Target = collision.gameObject,
            RicochetCount = _hitCount,
            Damage = _config.Damage
        });

        if (_hitCount >= _config.MaxCollisionCount)
            ReturnToPool();
    }

    private void ReturnToPool()
    {
        ReturnedToPool?.Invoke(this);
        _pool.Return(this);
    }
}
