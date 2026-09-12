using System;

namespace DreamVille.Dialogue
{
    /// <summary>The part of the dialogue other layers care about: is a conversation on screen.</summary>
    public interface IDialogueState
    {
        bool IsActive { get; }

        event Action<bool> ActiveChanged;
    }
}
