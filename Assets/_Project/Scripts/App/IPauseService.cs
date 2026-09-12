namespace DreamVille.App
{
    /// <summary>Owns the paused state of a level: time, player control and cursor mode.</summary>
    public interface IPauseService
    {
        bool IsPaused { get; }

        void Pause();

        void Resume();

        /// <summary>Takes control away without stopping time — used while a dialogue runs.</summary>
        void SetPlayerControl(bool isEnabled);
    }
}
