using DreamVille.Configs;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>Spawns bullets from a scene-scoped pool.</summary>
    public interface IBulletSpawner
    {
        /// <summary>Takes a bullet from the pool and sends it along <paramref name="shot"/> with <paramref name="config"/>'s tuning.</summary>
        void Fire(in ShotRay shot, BulletConfig config);
    }
}
