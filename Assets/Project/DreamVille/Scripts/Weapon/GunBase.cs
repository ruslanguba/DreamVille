using System;
using UnityEngine;

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

    public bool CanShoot() => _shootingTimer <= 0;

    public abstract void TryShoot();

    public virtual void Activate() { }
    public virtual void Deactivate() { }
}
