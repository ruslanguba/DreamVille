using UnityEngine;

using DreamVille.Gameplay.Damage;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    public class BulletDamageHandler : MonoBehaviour
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
            if (info.Target.TryGetComponent(out IDamageable d))
                d.TakeDamage(info.Damage, info.RicochetCount);
        }
    }
}
