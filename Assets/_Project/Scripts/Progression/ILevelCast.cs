using DreamVille.Gameplay.Npc;

namespace DreamVille.Progression
{
    /// <summary>
    /// The level's participants in its finale. Implemented by the scene's <see cref="LevelActors"/>; faked in tests.
    /// Both members return a real null when unassigned or destroyed, never Unity's fake null.
    /// </summary>
    public interface ILevelCast
    {
        /// <summary>The zone that opens the path to the next level, or null when the level has none.</summary>
        IFinalZone FinalZone { get; }

        /// <summary>The NPC standing in the final zone that reacts to the level outro, or null.</summary>
        INpcReactions FinalNpc { get; }
    }
}
