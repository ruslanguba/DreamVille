using UnityEngine;

using DreamVille.Core.Input;

namespace DreamVille.Gameplay.Weapons
{
    public class WeaponController: MonoBehaviour
    {
        [SerializeField] private GunBase _currentGun;
        private IPlayerInput _input;

        private void Awake()
        {
            _input = GetComponent<IPlayerInput>();
        }

        private void OnEnable()
        {
            _currentGun?.Activate();
            _input.FirePressed += Fire;
        }

        private void OnDisable()
        {
            _input.FirePressed -= Fire;
            _currentGun?.Deactivate();
        }

        private void Fire()
        {
            if (_currentGun.CanShoot())
                _currentGun.TryShoot();
        }
    }
}
