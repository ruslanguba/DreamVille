using DreamVille.App;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="ICursorService"/>. Records the most
    /// recently requested cursor mode.</summary>
    public sealed class FakeCursorService : ICursorService
    {
        public enum Mode
        {
            None,
            Gameplay,
            Ui
        }

        public Mode LastMode { get; private set; } = Mode.None;

        public void UseGameplayMode() => LastMode = Mode.Gameplay;

        public void UseUiMode() => LastMode = Mode.Ui;
    }
}
