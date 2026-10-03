using System;
using System.Collections.Generic;

using DreamVille.UI.Dialogues;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// Records what the presenter asked the dialogue canvas to do ("Show", "Hide", "ShowLine", in order, plus the
    /// lines themselves); raises the Next command on request and counts the handlers attached to it.
    /// </summary>
    public sealed class FakeDialogueView : IDialogueView
    {
        private readonly List<string> _calls = new List<string>();
        private readonly List<(bool SpeakerOnRight, string Text)> _lines = new List<(bool SpeakerOnRight, string Text)>();

        public event Action NextRequested;

        public IReadOnlyList<string> Calls => _calls;

        public IReadOnlyList<(bool SpeakerOnRight, string Text)> Lines => _lines;

        public int NextRequestedHandlerCount => NextRequested?.GetInvocationList().Length ?? 0;

        public void Show()
        {
            _calls.Add(nameof(Show));
        }

        public void Hide()
        {
            _calls.Add(nameof(Hide));
        }

        public void ShowLine(bool speakerOnRight, string text)
        {
            _calls.Add(nameof(ShowLine));
            _lines.Add((speakerOnRight, text));
        }

        public void PressNext()
        {
            NextRequested?.Invoke();
        }
    }
}
