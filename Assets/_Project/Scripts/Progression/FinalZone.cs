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
    public sealed class FinalZone : MonoBehaviour, IFinalZone
    {
        /// <inheritdoc />
        public event Action DialogueStarted;

        /// <inheritdoc />
        public event Action DialogueEnded;

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

            // The zone ignores the player until the level is won. It is disarmed rather than deactivated, so the
            // trigger stays subscribed to its zone and simply ignores the player until armed.
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

        /// <inheritdoc />
        public void Arm() => SetArmed(true);

        /// <inheritdoc />
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

        private void OnDialogueEnded() => DialogueEnded?.Invoke();
    }
}
