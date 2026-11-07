namespace DreamVille.Core.Pause
{
    /// <summary>Whether the level is paused; read by input that must stay silent under the pause menu.</summary>
    public interface IPauseState
    {
        bool IsPaused { get; }
    }
}
