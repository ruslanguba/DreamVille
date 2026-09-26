namespace DreamVille.UI.Hud
{
    /// <summary>Row of pillow icons.</summary>
    public interface IPillowView
    {
        /// <summary>Shows <paramref name="total"/> icons; the first <paramref name="inHand"/> look available.</summary>
        void Show(int total, int inHand);
    }
}
