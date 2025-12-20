using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [SerializeField] private BulletPool _pool;
    [SerializeField] private BulletEventRelay _eventRelay;

    public Bullet SpawnBullet(Vector3 position, Quaternion rotation, BulletConfig bulletConfig)
    {
        Debug.Log("Bullet spawned");
        Bullet bullet = _pool.Get();
        bullet.transform.SetPositionAndRotation(position, rotation);

        bullet.Init(bulletConfig);
        _eventRelay.Register(bullet);
        return bullet;
    }
}
