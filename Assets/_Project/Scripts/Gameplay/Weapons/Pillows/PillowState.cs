namespace DreamVille.Gameplay.Weapons.Pillows
{
    /// <summary>Where one pillow is in its throw-and-recall cycle.</summary>
    public enum PillowState
    {
        /// <summary>In hand: inactive, ready to be thrown.</summary>
        Stowed,

        /// <summary>Thrown and neither frozen nor recalled, including after it came to rest.</summary>
        Flying,

        /// <summary>Frozen in place as a platform.</summary>
        Placed,

        /// <summary>On its way back to the return point.</summary>
        Returning
    }
}
