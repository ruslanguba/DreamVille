namespace DreamVille.UI.Hud
{
    /// <summary>The gameplay HUD as the presenter sees it: five independent, passive blocks.</summary>
    public interface IGameHudView
    {
        IScoreView Score { get; }
        IStarsView Stars { get; }
        IPillowView Pillows { get; }
        IEnemyCounterView EnemyCounter { get; }
        IRicochetView Ricochet { get; }
    }
}
