namespace DreamVille.Gameplay.Weapons.Pillow
{
    /// <summary>What a throw-or-recall press did, so the caller knows whether to start the recall loop.</summary>
    public enum PillowCommand
    {
        /// <summary>The press was ignored.</summary>
        None,

        /// <summary>A stowed pillow was thrown.</summary>
        Thrown,

        /// <summary>A thrown pillow started returning.</summary>
        ReturnStarted
    }
}
