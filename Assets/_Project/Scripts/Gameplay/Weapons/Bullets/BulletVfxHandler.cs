using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>
    /// Plays the bullet's hit effect at each hit point and clears its trails when the pooled bullet is disabled.
    /// The effect is detached from the bullet in Awake and simulates in world space, so every burst stays where it
    /// hit, including the last one, on which the bullet returns to its pool. It is destroyed with the bullet.
    /// </summary>
    [RequireComponent(typeof(Bullet))]
    public sealed class BulletVfxHandler : MonoBehaviour
    {
        [SerializeField] private ParticleSystem _hitEffect;
        [SerializeField] private TrailRenderer[] _trails;
        private Bullet _bullet;

        private void Awake()
        {
            _bullet = GetComponent<Bullet>();

            if (_hitEffect == null)
            {
                Debug.LogError($"[{nameof(BulletVfxHandler)}] '{name}': {nameof(_hitEffect)} is not assigned; hits play no effect.", this);
                return;
            }

            // Deactivating the pooled bullet would otherwise cut its last burst short.
            _hitEffect.transform.SetParent(null, true);
        }

        private void OnEnable()
        {
            _bullet.Hit += OnHit;
        }

        private void OnDisable()
        {
            foreach (TrailRenderer trail in _trails)
            {
                trail.Clear();
            }

            _bullet.Hit -= OnHit;
        }

        private void OnDestroy()
        {
            if (_hitEffect != null)
                Destroy(_hitEffect.gameObject);
        }

        private void OnHit(BulletHitInfo info)
        {
            // A missing effect is reported once, in Awake.
            if (_hitEffect == null)
                return;

            _hitEffect.transform.position = info.HitPoint;
            _hitEffect.Play();
        }
    }
}
