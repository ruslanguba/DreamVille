using UnityEngine;
using Zenject;

using DreamVille.Input;

namespace DreamVille.Gameplay.Weapons
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private GunBase _currentGun;

        private IWeapon Weapon => _currentGun;

        private IPlayerInput _input;

        [Inject]
        private void Construct(IPlayerInput input)
        {
            _input = input;
        }

        // Gun visibility is controlled by the pickup (CollectableGun), not by the input controller.
        private void OnEnable()
        {
            _input.FirePressed += Fire;
        }

        private void OnDisable()
        {
            _input.FirePressed -= Fire;
        }

        private void Fire()
        {
            // Compares the serialized Component reference, not the interface, so a destroyed
            // gun is caught by Unity's overridden null check instead of producing a fake non-null.
            if (_currentGun != null && Weapon.CanShoot())
                Weapon.TryShoot();
        }
    }
}
