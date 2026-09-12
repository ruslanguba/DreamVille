using UnityEngine;

namespace DreamVille.Gameplay.Enemies.Combat
{
    /// <summary>No-op attack seam for enemies configured with <c>EnemyAttackKind.None</c>.</summary>
    public sealed class NullEnemyAttack : IEnemyAttack
    {
        public Vector3 Origin => Vector3.zero;

        public void Throw(Vector3 direction) { }
    }
}
