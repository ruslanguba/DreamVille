namespace DreamVille.App
{
    /// <summary>Ends the application; in the editor, leaves Play Mode instead.</summary>
    public interface IApplicationQuitter
    {
        void Quit();
    }
}
