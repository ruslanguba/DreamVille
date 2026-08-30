using System;
using UnityEngine;

namespace DreamVille.Gameplay.Weapons
{
    public abstract class GunBase : MonoBehaviour, IWeapon
    {
        [SerializeField] protected Transform _spawn;
        [SerializeField] protected float _shotPeriod = 0.5f;

        protected float _shootingTimer;

        protected virtual void Update()
        {
            if (_shootingTimer > 0)
                _shootingTimer -= Time.deltaTime;
        }

        // Выключенное оружие не стреляет: метод можно вызвать и у неактивного объекта.
        public bool CanShoot() => isActiveAndEnabled && _shootingTimer <= 0;

        public abstract void TryShoot();

        public virtual void Activate() => gameObject.SetActive(true);

        public virtual void Deactivate() => gameObject.SetActive(false);
    }
}
