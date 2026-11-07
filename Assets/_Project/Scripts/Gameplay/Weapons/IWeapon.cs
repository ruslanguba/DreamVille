namespace DreamVille.Gameplay.Weapons
{
    /// <summary>A weapon the player can fire; driven by <see cref="WeaponController"/>.</summary>
    public interface IWeapon
    {
        /// <summary>Fires when the weapon is equipped and its cooldown has elapsed; returns whether a shot left the muzzle.</summary>
        bool TryShoot();
    }
}
