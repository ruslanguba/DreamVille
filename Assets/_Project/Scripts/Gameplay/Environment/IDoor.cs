namespace DreamVille.Gameplay.Environment
{
    /// <summary>A one-way door driven by level logic.</summary>
    public interface IDoor
    {
        bool IsOpen { get; }

        /// <summary>Opens the door; later calls do nothing.</summary>
        void Open();
    }
}
