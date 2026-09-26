using DreamVille.UI.Hud;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>The whole HUD as five recording views; exposes the concrete fakes for assertions.</summary>
    public sealed class FakeGameHudView : IGameHudView
    {
        /// <summary>Badges x2..x6, as authored in the gameplay HUD prefab.</summary>
        public const int DefaultBadgeCount = 5;

        public FakeGameHudView(int badgeCount = DefaultBadgeCount)
        {
            RicochetFake = new FakeRicochetView(badgeCount);
        }

        public FakeScoreView ScoreFake { get; } = new FakeScoreView();
        public FakeStarsView StarsFake { get; } = new FakeStarsView();
        public FakePillowView PillowsFake { get; } = new FakePillowView();
        public FakeEnemyCounterView EnemyCounterFake { get; } = new FakeEnemyCounterView();
        public FakeRicochetView RicochetFake { get; }

        public IScoreView Score => ScoreFake;
        public IStarsView Stars => StarsFake;
        public IPillowView Pillows => PillowsFake;
        public IEnemyCounterView EnemyCounter => EnemyCounterFake;
        public IRicochetView Ricochet => RicochetFake;

        /// <summary>Total calls received by all five views.</summary>
        public int CallCount =>
            ScoreFake.Shown.Count
            + StarsFake.Progress.Count
            + StarsFake.Unlocked.Count
            + PillowsFake.Shown.Count
            + EnemyCounterFake.Shown.Count
            + RicochetFake.Shown.Count;
    }
}
