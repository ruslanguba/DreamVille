using System;
using Zenject;

using DreamVille.Dialogue;
using DreamVille.Progression.Objectives;

namespace DreamVille.Progression.Lobby
{
    /// <summary>
    /// The lobby's two-quest scenario in one place. Participants do not know about each other:
    /// the first dialogue opens the first door; clearing the first group unlocks the second dialogue;
    /// clearing the second group opens the second door; the guide reacts throughout.
    /// </summary>
    /// <remarks>
    /// Synchronous by design: the only timed beat (applause, then talk) belongs to the guide.
    /// <see cref="Initialize"/> never touches the guide, because the context kernel's Start may run
    /// before the guide's own Start applies its authored pose.
    /// </remarks>
    public sealed class LobbyDirector : IInitializable, IDisposable
    {
        private readonly ILobbyCast _cast;

        private KillGroupObjective _firstGroup;
        private KillGroupObjective _secondGroup;
        private bool _isInitialized;

        public LobbyDirector(ILobbyCast cast)
        {
            _cast = cast ?? throw new ArgumentNullException(nameof(cast));
        }

        public void Initialize()
        {
            _firstGroup = new KillGroupObjective(_cast.FirstGroup);
            _secondGroup = new KillGroupObjective(_cast.SecondGroup);

            _cast.SecondDialogue.IsArmed = false;

            _cast.FirstDialogue.DialogueStarted += OnFirstDialogueStarted;
            _cast.FirstDialogue.DialogueEnded += OnFirstDialogueEnded;
            _cast.SecondDialogue.DialogueStarted += OnSecondDialogueStarted;
            _cast.SecondDialogue.DialogueEnded += OnSecondDialogueEnded;
            _firstGroup.Completed += OnFirstGroupCleared;
            _secondGroup.Completed += OnSecondGroupCleared;
            _isInitialized = true;

            _firstGroup.Start();
            _secondGroup.Start();
        }

        public void Dispose()
        {
            if (!_isInitialized)
                return;

            _isInitialized = false;

            // A trigger with deleteOnEnd has destroyed itself after its dialogue; the cast then reports it as null,
            // and a destroyed trigger can never raise again, so there is nothing to detach from.
            IDialogueTrigger firstDialogue = _cast.FirstDialogue;
            if (firstDialogue != null)
            {
                firstDialogue.DialogueStarted -= OnFirstDialogueStarted;
                firstDialogue.DialogueEnded -= OnFirstDialogueEnded;
            }

            IDialogueTrigger secondDialogue = _cast.SecondDialogue;
            if (secondDialogue != null)
            {
                secondDialogue.DialogueStarted -= OnSecondDialogueStarted;
                secondDialogue.DialogueEnded -= OnSecondDialogueEnded;
            }

            _firstGroup.Completed -= OnFirstGroupCleared;
            _secondGroup.Completed -= OnSecondGroupCleared;

            _firstGroup.Dispose();
            _secondGroup.Dispose();
        }

        private void OnFirstDialogueStarted()
        {
            _cast.Guide.SetTalk();
        }

        private void OnFirstDialogueEnded()
        {
            _cast.FirstDoor.Open();
            _cast.Guide.SetIdle();
        }

        private void OnFirstGroupCleared()
        {
            _cast.SecondDialogue.IsArmed = true;
            _cast.Guide.SetGreeting();
        }

        private void OnSecondDialogueStarted()
        {
            _cast.Guide.SetApplause();
        }

        private void OnSecondDialogueEnded()
        {
            _cast.Guide.SetIdle();
        }

        private void OnSecondGroupCleared()
        {
            _cast.SecondDoor.Open();
            _cast.Guide.SetIdle();
        }
    }
}
