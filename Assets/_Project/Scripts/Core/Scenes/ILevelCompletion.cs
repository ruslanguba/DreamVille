namespace DreamVille.Core.Scenes
{
    /// <summary>
    /// Lets a scene object complete the current level without knowing about progress or saves,
    /// e.g. a hub's exit portal that counts as finishing it.
    /// </summary>
    public interface ILevelCompletion
    {
        /// <summary>Marks the level of the running scene completed. Repeated calls change nothing.</summary>
        void CompleteLevel();
    }
}
