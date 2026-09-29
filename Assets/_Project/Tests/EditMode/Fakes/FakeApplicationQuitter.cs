using DreamVille.App;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Counts quit requests instead of ending Play Mode.</summary>
    public sealed class FakeApplicationQuitter : IApplicationQuitter
    {
        public int QuitCount { get; private set; }

        public void Quit()
        {
            QuitCount++;
        }
    }
}
