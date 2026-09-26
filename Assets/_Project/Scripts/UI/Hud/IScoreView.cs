namespace DreamVille.UI.Hud
{
    /// <summary>Pop-up with the score just added. A new value restarts its visibility timer.</summary>
    public interface IScoreView
    {
        void ShowAdded(float delta);
    }
}
