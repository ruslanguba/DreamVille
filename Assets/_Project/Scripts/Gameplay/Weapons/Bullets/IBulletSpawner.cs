using UnityEngine;

using DreamVille.Configs;

namespace DreamVille.Gameplay.Weapons.Bullets
{
    /// <summary>Spawns bullets from a scene-scoped pool.</summary>
    public interface IBulletSpawner
    {
        /// <summary>Gets a bullet from the pool, positions it and arms it with <paramref name="bulletConfig"/>.</summary>
        Bullet SpawnBullet(Vector3 position, Quaternion rotation, BulletConfig bulletConfig);
    }
}
