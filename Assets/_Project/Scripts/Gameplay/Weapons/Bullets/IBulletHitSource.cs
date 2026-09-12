using System;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>Every bullet hit of the current scene, collapsed into one event for presenters.</summary>
    public interface IBulletHitSource
    {
        event Action<BulletHitInfo> BulletHit;
    }
}
