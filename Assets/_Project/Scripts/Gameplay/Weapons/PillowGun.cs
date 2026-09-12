using UnityEngine;
using Zenject;

using DreamVille.Audio;
using DreamVille.Configs;
using DreamVille.Gameplay.Weapons.Bullets;

namespace DreamVille.Gameplay.Weapons
{
    public class PillowGun : GunBase
    {

        [SerializeField] private BulletConfig _config;
        [SerializeField] private Transform _character;

        private IBulletSpawner _spawner;
        private ActorAudio _audio;

        [Inject]
        public void Construct(IBulletSpawner spawner)
        {
            _spawner = spawner;
        }

        private void Awake()
        {
            _audio = GetComponentInParent<ActorAudio>();
        }

        public override void TryShoot()
        {
            if (!CanShoot() || _spawn == null || _character == null) return;

            if (_config == null)
            {
                Debug.LogWarning($"[{nameof(PillowGun)}] {nameof(_config)} is not assigned.", this);
                return;
            }

            Bullet bullet = _spawner.SpawnBullet(_spawn.position, _spawn.rotation, _config);
            bullet.Launch(_character.forward);

            _audio?.PlayAttack();
            _shootingTimer = _shotPeriod;
        }
    }
}
