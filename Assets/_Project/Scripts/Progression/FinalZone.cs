using System;
using UnityEngine;

using DreamVille.Dialogue;

namespace DreamVille.Progression
{
    /// <summary>
    /// The level's final zone: a self-contained object that plays its own dialogue and can
    /// open the path onward. It knows nothing about progress, NPCs or the wider scenario;
    /// those links are created by <see cref="LevelDirector"/>.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class FinalZone : MonoBehaviour
    {
        private const string OpenTrigger = "start";

        /// <summary>The final dialogue started; a nearby NPC can react to it.</summary>
        public event Action DialogueStarted;

        /// <summary>The final dialogue finished; the path onward can be opened.</summary>
        public event Action DialogueFinished;

        [SerializeField] private DialogueTrigger _dialogue;
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _audio;
        [SerializeField] private AudioClip[] _mumbleClips;

        private void Awake()
        {
            // References within a single prefab: includeInactive and a null check are required.
            if (_dialogue == null) _dialogue = GetComponentInChildren<DialogueTrigger>(true);
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_audio == null) _audio = GetComponent<AudioSource>();

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
            if (_audio != null)
                _audio.Stop();

            if (_animator != null)
                _animator.SetTrigger(OpenTrigger);
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
            if (_audio == null || _mumbleClips == null || _mumbleClips.Length == 0)
                return;

            _audio.PlayOneShot(_mumbleClips[UnityEngine.Random.Range(0, _mumbleClips.Length)]);
        }

        private void OnDialogueEnded() => DialogueFinished?.Invoke();
    }
}
