namespace DreamVille.Configs.Enemies
{
    /// <summary>How an enemy walks its patrol route.</summary>
    public enum PatrolMode
    {
        /// <summary>Wraps back to the first point after the last one.</summary>
        Loop,

        /// <summary>Stops steering once the last point is reached.</summary>
        Once
    }
}
