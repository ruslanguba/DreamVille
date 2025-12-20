using System;
using UnityEngine;

public class BulletEventRelay : MonoBehaviour
{
    public event Action<BulletHitInfo> BulletHit;

    public void Register(Bullet bullet)
    {
        Debug.Log("Bullet registered");
        bullet.Hit += OnBulletHit;
        bullet.ReturnedToPool += Unregister;
    }

    private void OnBulletHit(BulletHitInfo info)
    {
        Debug.Log("BulletEventRelay OnBulletHit");
        BulletHit?.Invoke(info);
    }

    private void Unregister(Bullet bullet)
    {
        bullet.Hit -= OnBulletHit;
        bullet.ReturnedToPool -= Unregister;
    }
}
