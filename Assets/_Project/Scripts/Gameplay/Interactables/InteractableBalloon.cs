using UnityEngine;
using Zenject;

using DreamVille.Audio;
using DreamVille.Gameplay.Environment;

namespace DreamVille.Gameplay.Interactables
{
    public class InteractableBalloon : InteractableBase
    {
        [SerializeField] private AudioClip _clip;
        private PropsMover _propsMover;
        private AudioManager _audio;

        [Inject]
        public void Construct(AudioManager audio)
        {
            _audio = audio;
        }

        private void Awake()
        {
            _propsMover= GetComponentInParent<PropsMover>();
        }
        public override void OnInteract()
        {
            _audio?.PlaySFX(_clip);
            _propsMover.GetHit();
        }
    }
}
