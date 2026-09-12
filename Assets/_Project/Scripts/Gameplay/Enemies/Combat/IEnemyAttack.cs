using UnityEngine;

namespace DreamVille.Gameplay.Enemies.Combat
{
    /// <summary>An enemy's ranged attack seam.</summary>
    public interface IEnemyAttack
    {
        /// <summary>World point a thrown projectile is spawned from.</summary>
        Vector3 Origin { get; }

        void Throw(Vector3 direction);
    }
}
