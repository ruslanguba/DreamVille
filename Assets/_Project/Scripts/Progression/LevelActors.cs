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
    public sealed class LevelActors : MonoBehaviour, ILevelCast
    {
        [SerializeField] private FinalZone _finalZone;
        [SerializeField] private NpcReactionController _finalNpc;

        /// <inheritdoc />
        public IFinalZone FinalZone => _finalZone != null ? _finalZone : null;

        /// <inheritdoc />
        public INpcReactions FinalNpc => _finalNpc != null ? _finalNpc : null;
    }
}
