using System;

using DreamVille.Input;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Menu keys pressed on request by the test.</summary>
    public sealed class FakeMenuInput : IMenuInput
    {
        public event Action PausePressed;
        public event Action ComicsPressed;

        public void PressPause()
        {
            PausePressed?.Invoke();
        }

        public void PressComics()
        {
            ComicsPressed?.Invoke();
        }
    }
}
