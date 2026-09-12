using UnityEngine;

namespace DreamVille.Gameplay.Player.Animation
{
    /// <summary>Drives the character's animator parameters through cached hashes; tolerant of a null animator between skin swaps.</summary>
    public sealed class PlayerAnimatorDriver : IPlayerAnimator
    {
        private Animator _animator;

        public PlayerAnimatorDriver(Animator animator)
        {
            _animator = animator;
        }

        public void SetAnimator(Animator animator)
        {
            _animator = animator;
        }

        public void SetLocomotion(float speed, in LocomotionState state)
        {
            if (_animator == null)
                return;

            _animator.SetFloat(AnimParams.Speed, Mathf.Abs(speed));
            _animator.SetBool(AnimParams.IsMoving, state.IsMoving);
            _animator.SetInteger(AnimParams.MoveDirection, (int)state.Direction);
            _animator.SetFloat(AnimParams.AnimSpeedMultiplier, state.SpeedMultiplier);
        }

        public void TriggerJump()
        {
            if (_animator == null)
                return;

            _animator.SetTrigger(AnimParams.IsJump);
        }
    }
}
