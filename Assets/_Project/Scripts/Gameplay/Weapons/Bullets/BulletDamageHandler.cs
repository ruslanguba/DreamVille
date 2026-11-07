using UnityEngine;

using DreamVille.Gameplay.Damage;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>Applies the bullet's damage to the IDamageable it hits, passing on the shot's hit count.</summary>
    [RequireComponent(typeof(Bullet))]
    public sealed class BulletDamageHandler : MonoBehaviour
    {
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

        private void OnHit(BulletHitInfo info)
        {
            if (info.Target.TryGetComponent(out IDamageable damageable))
                damageable.TakeDamage(info.Damage, info.HitCount);
        }
    }
}
