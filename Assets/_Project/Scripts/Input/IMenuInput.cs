using System;

namespace DreamVille.Input
{
    /// <summary>The two menu keys: Esc toggles the pause menu, Tab toggles the comics page.</summary>
    public interface IMenuInput
    {
        event Action PausePressed;

        event Action ComicsPressed;
    }
}
