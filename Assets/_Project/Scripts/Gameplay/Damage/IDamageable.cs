using UnityEngine;

namespace DreamVille.Gameplay.Damage
{
    public interface IDamageable
    {
        void TakeDamage(float damage, int hitCount);
    }
}
