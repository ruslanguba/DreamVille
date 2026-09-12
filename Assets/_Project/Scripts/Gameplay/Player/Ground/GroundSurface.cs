using System;

namespace DreamVille.Gameplay.Player.Ground
{
    /// <summary>Kinds of special surface the ground sensor can detect under the player.</summary>
    [Flags]
    public enum GroundSurface
    {
        None = 0,
        Pillow = 1 << 0,
        RotatingPlatform = 1 << 1
    }
}
