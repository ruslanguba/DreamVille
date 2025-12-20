using UnityEngine;

public class PillowGun : GunBase
{

    [SerializeField] private BulletConfig _config;
    [SerializeField] private Transform _character;
    [SerializeField] private BulletSpawner _spawner;
    private ActorAudio _audio;
    private void Awake()
    {
        _audio = GetComponentInParent<ActorAudio>();
    }
    public void SetSpawner(BulletSpawner spawner)
    {
        _spawner = spawner;
    }

    public override void TryShoot()
    {
        if (!CanShoot() || _spawn == null || _character == null) return;

        Bullet bullet = _spawner.SpawnBullet(_spawn.position, _spawn.rotation, _config);
        bullet.Launch(_character.forward);

        _audio?.PlayAttack();
        _shootingTimer = _shotPeriod;
    }
}
