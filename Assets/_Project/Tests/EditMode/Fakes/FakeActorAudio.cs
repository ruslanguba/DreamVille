using DreamVille.Audio;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="IActorAudio"/>.</summary>
    public sealed class FakeActorAudio : IActorAudio
    {
        public int JumpCount { get; private set; }

        public int AttackCount { get; private set; }

        public int DeathCount { get; private set; }

        public int FootstepTickCount { get; private set; }

        public bool LastFootstepsMoving { get; private set; }

        public void PlayJump() => JumpCount++;

        public void PlayAttack() => AttackCount++;

        public void PlayDeath() => DeathCount++;

        public void TickFootsteps(bool isMoving)
        {
            FootstepTickCount++;
            LastFootstepsMoving = isMoving;
        }
    }
}
