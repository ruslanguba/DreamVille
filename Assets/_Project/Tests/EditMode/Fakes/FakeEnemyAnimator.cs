using DreamVille.Gameplay.Enemies.Presentation;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="IEnemyAnimator"/>.</summary>
    public sealed class FakeEnemyAnimator : IEnemyAnimator
    {
        public bool? IsMoving { get; private set; }

        public bool DeathTriggered { get; private set; }

        public float? LastLocomotionOffset { get; private set; }

        public void SetMoving(bool isMoving) => IsMoving = isMoving;

        public void TriggerDeath() => DeathTriggered = true;

        public void PlayLocomotion(float normalizedOffset) => LastLocomotionOffset = normalizedOffset;
    }
}
