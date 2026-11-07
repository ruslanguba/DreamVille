using DreamVille.Gameplay.Npc;
using DreamVille.Progression;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written <see cref="ILevelCast"/> double; null members mean "no finale".</summary>
    public sealed class FakeLevelCast : ILevelCast
    {
        public FakeLevelCast(IFinalZone finalZone = null, INpcReactions finalNpc = null)
        {
            FinalZone = finalZone;
            FinalNpc = finalNpc;
        }

        public IFinalZone FinalZone { get; }

        public INpcReactions FinalNpc { get; }
    }
}
