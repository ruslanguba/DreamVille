namespace DreamVille.Gameplay.NPC
{
    /// <summary>Reactions an NPC can play in response to level events.</summary>
    public interface INpcReactions
    {
        /// <summary>Greets the player, e.g. when the level has just been won.</summary>
        void SetGreeting();

        /// <summary>Applauds, then falls into talking after a short delay.</summary>
        void SetApplause();

        /// <summary>Returns to the idle pose and stops any reaction sound.</summary>
        void SetIdle();
    }
}
