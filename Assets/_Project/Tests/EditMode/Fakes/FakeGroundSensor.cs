using DreamVille.Gameplay.Player.Ground;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="IGroundSensor"/>.</summary>
    public sealed class FakeGroundSensor : IGroundSensor
    {
        public bool IsGrounded { get; set; }

        public GroundSurface Surface { get; set; } = GroundSurface.None;

        public int SampleCount { get; private set; }

        public bool IsSafeGround => IsGrounded && Surface == GroundSurface.None;

        public void Sample() => SampleCount++;
    }
}
