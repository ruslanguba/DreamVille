namespace DreamVille.UI.Hud
{
    /// <summary>Row of star meters. Out-of-range indices are ignored.</summary>
    public interface IStarsView
    {
        /// <summary>Sets the fill of one star meter.</summary>
        /// <param name="starIndex">Zero-based star index.</param>
        /// <param name="normalized">Fill of that star, 0..1.</param>
        void SetProgress(int starIndex, float normalized);

        /// <summary>Plays the unlock effect (particles and a scale pop) on one star.</summary>
        void PlayUnlock(int starIndex);
    }
}
