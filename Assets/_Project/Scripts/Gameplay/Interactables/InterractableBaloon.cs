using UnityEngine;

using DreamVille.Audio;
using DreamVille.Gameplay.Environment;

namespace DreamVille.Gameplay.Interactables
{
    public class InterractableBaloon : InteractableBase
    {
        [SerializeField] private AudioClip _clip;
        private PropsMover _propsMover;

        private void Awake()
        {
            _propsMover= GetComponentInParent<PropsMover>();
        }
        public override void OnInteract()
        {
            AudioManager.Instance.PlaySFX( _clip );
            _propsMover.GetHit();
        }
    }
}
