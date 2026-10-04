using UnityEngine.VFX;
using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>Plays the bullet's hit effect at the hit point and clears its trails when the pooled bullet is disabled.</summary>
    public class BulletVFXHandler : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _hitEffect;
        [SerializeField] private TrailRenderer[] _trailEffect;
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
            foreach (TrailRenderer trail in _trailEffect)
            {
                trail.Clear();
            }

            _bullet.Hit -= OnHit;
        }

        private void OnHit(BulletHitInfo info)
        {
            if (_hitEffect == null) return;
            _hitEffect.transform.position = info.HitPoint;
            _hitEffect.Play();
        }
    }
}
