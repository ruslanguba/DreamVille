using System.Collections.Generic;

using DreamVille.Dialogue;
using DreamVille.Gameplay.Damage;
using DreamVille.Gameplay.Environment;
using DreamVille.Gameplay.NPC;

namespace DreamVille.Progression.Lobby
{
    /// <summary>The lobby's participants. Implemented by the scene's <see cref="LobbyActors"/>; faked in tests.</summary>
    public interface ILobbyCast
    {
        /// <summary>Enemies whose deaths unlock the second dialogue.</summary>
        IReadOnlyList<IMortal> FirstGroup { get; }

        /// <summary>Enemies whose deaths open the second door.</summary>
        IReadOnlyList<IMortal> SecondGroup { get; }

        /// <summary>The guide's first talk; its end opens the first door.</summary>
        IDialogueTrigger FirstDialogue { get; }

        /// <summary>The guide's second talk; disarmed until the first group is cleared.</summary>
        IDialogueTrigger SecondDialogue { get; }

        IDoor FirstDoor { get; }

        IDoor SecondDoor { get; }

        /// <summary>The NPC who leads the player through the lobby.</summary>
        INpcReactions Guide { get; }
    }
}
