using System;
using Zenject;

using DreamVille.Core.Pause;
using DreamVille.Dialogue;

namespace DreamVille.UI.Dialogues
{
    /// <summary>
    /// Draws the conversation run by <see cref="IDialogueService"/> and sends the view's Next command back.
    /// Holds no dialogue rules: the line queue lives in the service, the advance input in <c>DialogueInputHandler</c>.
    /// </summary>
    public sealed class DialoguePresenter : IInitializable, IDisposable
    {
        private readonly IDialogueService _dialogue;
        private readonly IDialogueView _view;
        private readonly IPauseState _pause;

        public DialoguePresenter(IDialogueService dialogue, IDialogueView view, IPauseState pause)
        {
            _dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _pause = pause ?? throw new ArgumentNullException(nameof(pause));
        }

        public void Initialize()
        {
            _dialogue.ActiveChanged += OnActiveChanged;
            _dialogue.LineShown += OnLineShown;

            _view.NextRequested += OnNextRequested;
        }

        /// <summary>Unsubscribes only; never calls the view, which may already be destroyed with the scene.</summary>
        public void Dispose()
        {
            _dialogue.ActiveChanged -= OnActiveChanged;
            _dialogue.LineShown -= OnLineShown;

            _view.NextRequested -= OnNextRequested;
        }

        private void OnActiveChanged(bool isActive)
        {
            if (isActive)
                _view.Show();
            else
                _view.Hide();
        }

        private void OnLineShown(SpokenLine line)
        {
            _view.ShowLine(line.Speaker == DialogueSpeaker.Player, line.Text);
        }

        private void OnNextRequested()
        {
            // The pause menu owns the screen: a line must not advance, or end the dialogue, behind it.
            if (_pause.IsPaused)
                return;

            _dialogue.ShowNext();
        }
    }
}
