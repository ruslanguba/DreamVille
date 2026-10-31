using UnityEngine;

using DreamVille.Gameplay.Npc;

namespace DreamVille.Progression
{
    /// <summary>
    /// The level's cast: the single place where the scene lists its participants.
    /// Cross-references between scene objects are collected here instead, where they are
    /// visible as a whole. Auto-discovery does not work: a lobby can have two final zones
    /// and only one of them is the real one.
    /// Required in every gameplay scene, on its own scene object; empty fields mean "no finale".
    /// </summary>
    public class LevelActors : MonoBehaviour
    {
        [SerializeField] private FinalZone _finalZone;
        [SerializeField] private NpcReactionController _finalNpc;

        /// <summary>The zone that opens the path to the next level, or null when the level has none.</summary>
        public FinalZone FinalZone => _finalZone != null ? _finalZone : null;

        /// <summary>NPC standing in the final zone that reacts to the level outro, or null. A destroyed or
        /// unassigned component is returned as a real null so the interface never hides Unity's fake-null.</summary>
        public INpcReactions FinalNpc => _finalNpc != null ? _finalNpc : null;
    }
}
