using UnityEngine;

namespace DreamVille.Gameplay.Interactables
{
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        public abstract void OnInteract();
    }
}
