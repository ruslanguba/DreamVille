using UnityEngine;

using DreamVille.Gameplay.Player.Animation;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="IPlayerAnimator"/>.</summary>
    public sealed class FakePlayerAnimator : IPlayerAnimator
    {
        public Animator LastAnimator { get; private set; }

        public float LastSpeed { get; private set; }

        public LocomotionState LastLocomotionState { get; private set; }

        public int JumpTriggeredCount { get; private set; }

        public void SetAnimator(Animator animator) => LastAnimator = animator;

        public void SetLocomotion(float speed, in LocomotionState state)
        {
            LastSpeed = speed;
            LastLocomotionState = state;
        }

        public void TriggerJump() => JumpTriggeredCount++;
    }
}
