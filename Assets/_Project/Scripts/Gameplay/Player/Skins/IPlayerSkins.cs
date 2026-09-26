namespace DreamVille.Gameplay.Player.Skins
{
    /// <summary>The player's wardrobe: the seam a skin pickup reaches through the interaction's instigator.</summary>
    public interface IPlayerSkins
    {
        /// <summary>
        /// Activates skin <paramref name="index"/> and re-targets the player animator; an out-of-range index logs an
        /// error and changes nothing.
        /// </summary>
        void SetSkin(int index);
    }
}
