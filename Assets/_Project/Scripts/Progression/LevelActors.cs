using UnityEngine;

using DreamVille.Gameplay.NPC;

namespace DreamVille.Progression
{
    /// <summary>
    /// The level's cast: the single place where the scene lists its participants.
    /// Cross-references between scene objects are collected here instead, where they are
    /// visible as a whole. Auto-discovery does not work: a lobby can have two final zones
    /// and only one of them is the real one.
    /// </summary>
    public class LevelActors : MonoBehaviour
    {
        [SerializeField] private FinalZone _finalZone;
        [SerializeField] private NPCReactionController _finalNpc;

        /// <summary>The zone that opens the path to the next level. May be absent.</summary>
        public FinalZone FinalZone => _finalZone;

        /// <summary>NPC standing in the final zone that reacts to the level outro. Optional.</summary>
        public INpcReactions FinalNpc => _finalNpc;
    }
}
