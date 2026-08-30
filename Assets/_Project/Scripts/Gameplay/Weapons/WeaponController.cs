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

        // Видимостью оружия управляет подбор (CollectableGun), а не контроллер ввода.
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
            if (_currentGun != null && _currentGun.CanShoot())
                _currentGun.TryShoot();
        }
    }
}
