using UnityEngine;
using Zenject;

using DreamVille.Dialogue;
using DreamVille.Gameplay.Weapons.Pillows;

namespace DreamVille.Progression.Pillows
{
    /// <summary>
    /// Unlocks every pillow up to a configured number when a specific dialogue ends (the Lobby_02 pillow tutorial).
    /// Replaying the dialogue grants nothing new: <see cref="IPillowInventory.UnlockUpTo"/> never lowers or inflates the count.
    /// </summary>
    public sealed class PillowRewardOnDialogue : MonoBehaviour
    {
        [SerializeField] private DialogueTrigger _dialogue;

        [Tooltip("Every pillow up to this 1-based number is unlocked when the dialogue ends.")]
        [SerializeField, Min(1)] private int _pillowNumber = 2;

        private IPillowInventory _inventory;

        [Inject]
        public void Construct(IPillowInventory inventory)
        {
            _inventory = inventory;
        }

        private void Awake()
        {
            if (_dialogue == null)
            {
                Debug.LogError($"[{nameof(PillowRewardOnDialogue)}] {nameof(_dialogue)} is not assigned. The pillow reward is disabled.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_dialogue != null)
                _dialogue.DialogueEnded += OnDialogueEnded;
        }

        private void OnDisable()
        {
            // A trigger with deleteOnEnd is already destroyed here; it can never raise again, so there is nothing to detach.
            if (_dialogue != null)
                _dialogue.DialogueEnded -= OnDialogueEnded;
        }

        private void OnDialogueEnded()
        {
            _inventory.UnlockUpTo(_pillowNumber);
        }
    }
}
