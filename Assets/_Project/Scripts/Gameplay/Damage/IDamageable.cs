namespace DreamVille.Gameplay.Damage
{
    /// <summary>Something a shot or a hazard can hurt.</summary>
    public interface IDamageable
    {
        /// <summary>Applies <paramref name="damage"/>; <paramref name="hitCount"/> is the hit count of the shot (1 for a direct hit or a hazard, 2 after one ricochet).</summary>
        void TakeDamage(float damage, int hitCount);
    }
}
