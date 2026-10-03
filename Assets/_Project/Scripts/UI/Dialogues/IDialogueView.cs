using System;

namespace DreamVille.UI.Dialogues
{
    /// <summary>The dialogue canvas: one panel showing one line at a time.</summary>
    public interface IDialogueView
    {
        /// <summary>The player asked for the next line (Next button).</summary>
        event Action NextRequested;

        void Show();

        void Hide();

        /// <summary>Shows a line in the right speaker slot (player) or the left one (NPC).</summary>
        void ShowLine(bool speakerOnRight, string text);
    }
}
