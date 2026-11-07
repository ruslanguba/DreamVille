using System;
using UnityEngine;

using DreamVille.Dialogue;
using DreamVille.Gameplay;

namespace DreamVille.Progression
{
    /// <summary>
    /// The level's final zone: a self-contained object that plays its own dialogue and can
    /// open the path onward. It knows nothing about progress, NPCs or the wider scenario;
    /// those links are created by <see cref="LevelDirector"/>.
    /// </summary>
    [RequireComponent(typeof(Animator), typeof(AudioSource))]
    public class FinalZone : MonoBehaviour
    {
        /// <summary>The final dialogue started; a nearby NPC can react to it.</summary>
        public event Action DialogueStarted;

        /// <summary>The final dialogue finished; the path onward can be opened.</summary>
        public event Action DialogueFinished;

        // Serialized, not required: the trigger destroys itself after the finale (deleteOnEnd),
        // and Unity refuses to destroy a component another one requires.
        [Tooltip("The zone's own dialogue, usually on this object. Empty only on scenery copies that never arm.")]
        [SerializeField] private DialogueTrigger _dialogue;
        [SerializeField] private AudioClip[] _mumbleClips;

        private Animator _portalAnimator;
        private AudioSource _audioSource;

        private void Reset() => _dialogue = GetComponentInChildren<DialogueTrigger>(true);

        private void Awake()
        {
            _portalAnimator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();

            // The zone ignores the player until the level is won. The object stays active:
            // a disabled trigger lost its references and still caught the player's input.
            SetArmed(false);
        }

        private void OnEnable()
        {
            if (_dialogue == null)
                return;

            _dialogue.DialogueStarted += OnDialogueStarted;
            _dialogue.DialogueEnded += OnDialogueEnded;
        }

        private void OnDisable()
        {
            if (_dialogue == null)
                return;

            _dialogue.DialogueStarted -= OnDialogueStarted;
            _dialogue.DialogueEnded -= OnDialogueEnded;
        }

        /// <summary>Arms the zone: from this point the player's entry starts the final dialogue.</summary>
        public void Arm() => SetArmed(true);

        /// <summary>Plays the portal-opening animation.</summary>
        public void OpenPortal()
        {
            _audioSource.Stop();
            _portalAnimator.SetTrigger(AnimParams.Start);
        }

        private void SetArmed(bool isArmed)
        {
            if (_dialogue != null)
                _dialogue.IsArmed = isArmed;
        }

        private void OnDialogueStarted()
        {
            PlayMumble();
            DialogueStarted?.Invoke();
        }

        private void PlayMumble()
        {
            if (_mumbleClips == null || _mumbleClips.Length == 0)
                return;

            _audioSource.PlayOneShot(_mumbleClips[UnityEngine.Random.Range(0, _mumbleClips.Length)]);
        }

        private void OnDialogueEnded() => DialogueFinished?.Invoke();
    }
}
