using UnityEngine;

using DreamVille.Gameplay.Enemies;

namespace DreamVille.Gameplay.Player
{
    /// <summary>Kill volume: returns the player to the last safe position and deals damage to an enemy that falls in.</summary>
    public class DeathTrigger : MonoBehaviour
    {
        // Twice the toughest enemy's 10 HP: the fall always kills.
        private const float LethalDamage = 20f;

        // The kill scores like a direct hit: ScoreService multiplies the score per kill by this count.
        private const int KillHitCount = 1;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerRespawn respawn))
            {
                respawn.ReturnToSafePosition();
            }

            if (other.TryGetComponent(out EnemyHealth health))
            {
                health.TakeDamage(LethalDamage, KillHitCount);
            }
        }
    }
}
