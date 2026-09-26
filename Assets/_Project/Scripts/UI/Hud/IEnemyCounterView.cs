namespace DreamVille.UI.Hud
{
    /// <summary>Text with the number of enemies still alive.</summary>
    public interface IEnemyCounterView
    {
        void SetRemaining(int remaining);
    }
}
