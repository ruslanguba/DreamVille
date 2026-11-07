using System;

namespace DreamVille.Progression
{
    /// <summary>The level's final zone as the level scenario sees it. Implemented by <see cref="FinalZone"/>; faked in tests.</summary>
    public interface IFinalZone
    {
        /// <summary>The final dialogue started; a nearby NPC can react to it.</summary>
        event Action DialogueStarted;

        /// <summary>The final dialogue ended; the path onward can be opened.</summary>
        event Action DialogueEnded;

        /// <summary>Arms the zone: from this point the player's entry starts the final dialogue.</summary>
        void Arm();

        /// <summary>Plays the portal-opening animation.</summary>
        void OpenPortal();
    }
}
