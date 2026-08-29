using System;
using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    public class BulletEventRelay : MonoBehaviour
    {
        public event Action<BulletHitInfo> BulletHit;

        public void Register(Bullet bullet)
        {
            bullet.Hit += OnBulletHit;
            bullet.ReturnedToPool += Unregister;
        }

        private void OnBulletHit(BulletHitInfo info)
        {
            BulletHit?.Invoke(info);
        }

        private void Unregister(Bullet bullet)
        {
            bullet.Hit -= OnBulletHit;
            bullet.ReturnedToPool -= Unregister;
        }
    }
}
