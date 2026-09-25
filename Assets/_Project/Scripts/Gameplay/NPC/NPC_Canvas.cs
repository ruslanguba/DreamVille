using UnityEngine;

using DreamVille.Core.Players;

namespace DreamVille.Gameplay.NPC
{
    public class NPC_Canvas : MonoBehaviour
    {
        [SerializeField] private GameObject Canvas;

        private void Start()
        {
            Canvas.SetActive(false);
        }

        // Checks the collider's own GameObject only, so the player's child trigger spheres do not toggle the canvas.
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IPlayerMarker>(out _))
                Canvas.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IPlayerMarker>(out _))
                Canvas.SetActive(false);
        }
    }
}
