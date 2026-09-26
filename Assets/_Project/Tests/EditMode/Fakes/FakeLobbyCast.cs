using System.Collections.Generic;

using DreamVille.Dialogue;
using DreamVille.Gameplay.Damage;
using DreamVille.Gameplay.Environment;
using DreamVille.Gameplay.NPC;
using DreamVille.Progression.Lobby;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>
    /// A complete lobby cast of fakes: two enemies in the first group, one in the second.
    /// Members are exposed with their fake types so tests can kill enemies and raise dialogue events.
    /// </summary>
    public sealed class FakeLobbyCast : ILobbyCast
    {
        public FakeMortal FirstA { get; } = new FakeMortal();
        public FakeMortal FirstB { get; } = new FakeMortal();
        public FakeMortal Second { get; } = new FakeMortal();
        public FakeDialogueTrigger FirstTrigger { get; } = new FakeDialogueTrigger();
        public FakeDialogueTrigger SecondTrigger { get; } = new FakeDialogueTrigger();
        public FakeDoor FirstGate { get; } = new FakeDoor();
        public FakeDoor SecondGate { get; } = new FakeDoor();
        public FakeNpcReactions GuideLog { get; } = new FakeNpcReactions();

        public IReadOnlyList<IMortal> FirstGroup => new IMortal[] { FirstA, FirstB };
        public IReadOnlyList<IMortal> SecondGroup => new IMortal[] { Second };
        /// <summary>Simulates deleteOnEnd: the real cast reports a destroyed trigger as null.</summary>
        public bool FirstTriggerDestroyed { get; set; }
        public bool SecondTriggerDestroyed { get; set; }

        public IDialogueTrigger FirstDialogue => FirstTriggerDestroyed ? null : FirstTrigger;
        public IDialogueTrigger SecondDialogue => SecondTriggerDestroyed ? null : SecondTrigger;
        public IDoor FirstDoor => FirstGate;
        public IDoor SecondDoor => SecondGate;
        public INpcReactions Guide => GuideLog;
    }
}
