using DreamVille.Gameplay.Environment;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written <see cref="IDoor"/> double: counts every <see cref="Open"/> call, repeated ones included.</summary>
    public sealed class FakeDoor : IDoor
    {
        public bool IsOpen => OpenCount > 0;

        public int OpenCount { get; private set; }

        public void Open() => OpenCount++;
    }
}
