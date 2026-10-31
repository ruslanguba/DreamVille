using UnityEngine;

using DreamVille.Core.Players;

namespace DreamVille.Gameplay.Npc
{
    /// <summary>Shows the NPC's world-space canvas while the player stands in the trigger and hides it when the player leaves.</summary>
    public class NpcCanvasToggle : MonoBehaviour
    {
        [SerializeField] private GameObject _canvas;

        private void Start()
        {
            _canvas.SetActive(false);
        }

        // Checks the collider's own GameObject only, so the player's child trigger spheres do not toggle the canvas.
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<IPlayerMarker>(out _))
                _canvas.SetActive(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent<IPlayerMarker>(out _))
                _canvas.SetActive(false);
        }
    }
}
