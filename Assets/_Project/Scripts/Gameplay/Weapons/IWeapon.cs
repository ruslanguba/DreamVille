namespace DreamVille.Gameplay.Weapons
{
    /// <summary>A weapon the player can fire; driven by <see cref="WeaponController"/>.</summary>
    public interface IWeapon
    {
        /// <summary>True when the weapon is equipped and its cooldown has elapsed.</summary>
        bool CanShoot();

        /// <summary>Fires when <see cref="CanShoot"/> is true; returns whether a shot left the muzzle.</summary>
        bool TryShoot();
    }
}
