using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

using DreamVille.Audio;
using DreamVille.Configs;
using DreamVille.Gameplay.Weapons.Bullets;

namespace DreamVille.Gameplay.Weapons
{
    /// <summary>
    /// The player's gun: fires pooled bullets along the muzzle's forward on a timestamp cooldown.
    /// The aim preview reads the same <see cref="Aim"/> and <see cref="Projectile"/>, so it traces exactly this shot.
    /// </summary>
    public sealed class PillowGun : MonoBehaviour, IWeapon, IAimSource
    {
        [Tooltip("Where bullets leave the gun; its forward is the shot direction.")]
        [FormerlySerializedAs("_spawn")]
        [SerializeField] private Transform _muzzle;

        [Tooltip("Seconds between two shots.")]
        [FormerlySerializedAs("_shotPeriod")]
        [SerializeField, Min(0f)] private float _shotInterval = 0.5f;

        [SerializeField] private BulletConfig _config;

        private IBulletSpawner _spawner;
        private IActorAudio _audio;
        private ShotCooldown _cooldown;

        [Inject]
        public void Construct(IBulletSpawner spawner, IActorAudio audio)
        {
            _spawner = spawner;
            _audio = audio;
        }

        /// <inheritdoc />
        public bool CanAim => isActiveAndEnabled;   // Awake disables the gun when the muzzle or config is missing

        /// <inheritdoc />
        public ShotRay Aim => new ShotRay(_muzzle.position, _muzzle.forward);

        /// <inheritdoc />
        public BulletConfig Projectile => _config;

        private void Awake()
        {
            _cooldown = new ShotCooldown(_shotInterval);

            if (_muzzle == null || _config == null)
            {
                Debug.LogError($"[{nameof(PillowGun)}] {nameof(_muzzle)} and {nameof(_config)} must be assigned; the gun is disabled.", this);
                enabled = false;
            }
        }

        /// <inheritdoc />
        public bool CanShoot() => isActiveAndEnabled && _cooldown.IsReady(Time.time);

        /// <inheritdoc />
        public bool TryShoot()
        {
            if (!CanShoot())
                return false;

            ShotRay ray = Aim;
            Bullet bullet = _spawner.SpawnBullet(ray.Origin, ray.Rotation, _config);
            bullet.Launch(ray.Direction);

            _audio.PlayAttack();
            _cooldown.Consume(Time.time);
            return true;
        }

        /// <summary>Equips the gun (shows it and lets it fire); called by the gun pickup.</summary>
        public void Activate() => gameObject.SetActive(true);

        /// <summary>Unequips the gun; an inactive gun neither fires nor draws its aim preview.</summary>
        public void Deactivate() => gameObject.SetActive(false);
    }
}
