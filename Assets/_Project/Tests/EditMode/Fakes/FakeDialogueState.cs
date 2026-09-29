using System;

using DreamVille.Dialogue;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Dialogue state set by the test; <see cref="ActiveChanged"/> is raised by <see cref="SetActive"/>.</summary>
    public sealed class FakeDialogueState : IDialogueState
    {
        public event Action<bool> ActiveChanged;

        public bool IsActive { get; private set; }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
            ActiveChanged?.Invoke(isActive);
        }
    }
}
