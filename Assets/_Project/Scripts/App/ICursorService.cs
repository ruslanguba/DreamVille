namespace DreamVille.App
{
    /// <summary>Cursor visibility and lock mode for the two input contexts of the game.</summary>
    public interface ICursorService
    {
        void UseGameplayMode();

        void UseUiMode();
    }
}
