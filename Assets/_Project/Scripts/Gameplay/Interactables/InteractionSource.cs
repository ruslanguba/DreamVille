namespace DreamVille.Gameplay.Interactables
{
    /// <summary>What caused an interaction. Hosts accept one source and ignore the other.</summary>
    public enum InteractionSource
    {
        /// <summary>A player bullet collided with the object.</summary>
        Shot = 0,

        /// <summary>The player's pickup trigger overlapped the object.</summary>
        Touch = 1
    }
}
