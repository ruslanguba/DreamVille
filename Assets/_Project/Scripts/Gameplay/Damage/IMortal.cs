using System;

namespace DreamVille.Gameplay.Damage
{
    /// <summary>Something that dies once and announces it. The seam objectives count on.</summary>
    public interface IMortal
    {
        bool IsDead { get; }

        /// <summary>Raised once, with the sender, when it dies.</summary>
        event Action<IMortal> Died;
    }
}
