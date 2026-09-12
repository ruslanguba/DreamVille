using System;
using UnityEngine;

using DreamVille.Core.Players;

namespace DreamVille.Gameplay.Player
{
    /// <summary>Reports the player entering this trigger volume.</summary>
    public class PlayerDetectorTrigger : MonoBehaviour, IPlayerZone
    {
        public event Action PlayerEntered;

        private void OnTriggerEnter(Collider other)
        {
            if (PlayerDetection.IsPlayer(other))
                PlayerEntered?.Invoke();
        }
    }
}
