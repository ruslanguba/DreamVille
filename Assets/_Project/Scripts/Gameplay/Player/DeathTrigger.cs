using UnityEngine;

using DreamVille.Gameplay.Enemies;

namespace DreamVille.Gameplay.Player
{
    /// <summary>Kill volume: returns the player to the last safe position and deals damage to an enemy that falls in.</summary>
    public class DeathTrigger : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerRespawn respawn))
            {
                respawn.ReturnToSafePosition();
            }

            if (other.TryGetComponent(out EnemyHealth health))
            {
                health.TakeDamage(20, 1);
            }
        }
    }
}
