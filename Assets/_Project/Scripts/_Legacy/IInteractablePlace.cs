using UnityEngine;

namespace DreamVille.Legacy
{
    public interface IInteractablePlace
    {
        void OpenDoor();
        void CloseDoor();
        void SetEnemyVisibility(bool visible);
    }
}
