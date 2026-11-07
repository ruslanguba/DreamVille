using UnityEngine;

using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Player;

namespace DreamVille.Gameplay.Environment
{
    /// <summary>Kill volume: returns the player to the last safe position and kills an enemy that falls in.</summary>
    public sealed class DeathTrigger : MonoBehaviour
    {
        // The kill scores like a direct hit: ScoreService multiplies the score per kill by this count.
        private const int KillHitCount = 1;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerRespawn respawn))
            {
                respawn.ReturnToSafePosition();
            }

            // Kill, not damage: an enemy inside its spawn or flee immunity window must not fall forever.
            if (other.TryGetComponent(out EnemyHealth health))
            {
                health.Kill(KillHitCount);
            }
        }
    }
}
