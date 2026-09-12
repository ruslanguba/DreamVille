using DreamVille.Gameplay.Enemies.Presentation;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="IEnemyAudio"/>.</summary>
    public sealed class FakeEnemyAudio : IEnemyAudio
    {
        public int FootstepTickCount { get; private set; }

        public bool LastFootstepsMoving { get; private set; }

        public int AttackCount { get; private set; }

        public void TickFootsteps(bool isMoving)
        {
            FootstepTickCount++;
            LastFootstepsMoving = isMoving;
        }

        public void PlayAttack() => AttackCount++;
    }
}
