using DreamVille.Configs;

namespace DreamVille.Gameplay.Weapons
{
    /// <summary>What an aim preview needs to trace exactly the shot the weapon would fire now.</summary>
    public interface IAimSource
    {
        /// <summary>Origin and direction of the next shot.</summary>
        ShotRay Aim { get; }

        /// <summary>The projectile the next shot fires; its radius, contacts and travel bound the preview.</summary>
        BulletConfig Projectile { get; }
    }
}
