using DreamVille.Core.Players;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="IPlayerControlSwitch"/>. Counts calls and
    /// tracks the last requested state.</summary>
    public sealed class FakePlayerControlSwitch : IPlayerControlSwitch
    {
        public int EnableCalls { get; private set; }

        public int DisableCalls { get; private set; }

        public bool IsEnabled { get; private set; }

        public void EnableControl()
        {
            EnableCalls++;
            IsEnabled = true;
        }

        public void DisableControl()
        {
            DisableCalls++;
            IsEnabled = false;
        }
    }
}
