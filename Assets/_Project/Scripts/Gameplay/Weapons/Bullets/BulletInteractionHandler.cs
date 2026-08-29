using UnityEngine;

using DreamVille.Gameplay.Interactables;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    public class BulletInteractionHandler : MonoBehaviour
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
            if (info.Target.TryGetComponent(out IInteractable interactable))
                interactable.OnInteract();
        }
    }
}
