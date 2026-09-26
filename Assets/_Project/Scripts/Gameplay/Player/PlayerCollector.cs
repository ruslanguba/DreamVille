using UnityEngine;

using DreamVille.Gameplay.Interactables;

namespace DreamVille.Gameplay.Player
{
    /// <summary>The player's touch sender: every <see cref="IInteractable"/> its trigger overlaps receives a touch interaction.</summary>
    public sealed class PlayerCollector : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out IInteractable target))
                target.OnInteract(InteractionContext.Touch(transform));
        }
    }
}
