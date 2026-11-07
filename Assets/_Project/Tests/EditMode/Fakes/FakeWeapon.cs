using DreamVille.Gameplay.Weapons;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="IWeapon"/>: settable readiness, counts shot attempts.</summary>
    public sealed class FakeWeapon : IWeapon
    {
        public bool IsReady { get; set; } = true;

        public int TryShootCount { get; private set; }

        public bool TryShoot()
        {
            TryShootCount++;
            return IsReady;
        }
    }
}
