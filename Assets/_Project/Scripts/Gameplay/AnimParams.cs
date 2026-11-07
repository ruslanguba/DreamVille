using UnityEngine;

namespace DreamVille.Gameplay
{
    /// <summary>Cached animator hashes. Parameter and state names live here only.</summary>
    public static class AnimParams
    {
        public static readonly int Dead = Animator.StringToHash("dead");
        public static readonly int IsMoving = Animator.StringToHash("isMoving");
        public static readonly int Speed = Animator.StringToHash("speed");
        public static readonly int IsJump = Animator.StringToHash("isJump");
        public static readonly int MoveDirection = Animator.StringToHash("moveDirection");
        public static readonly int AnimSpeedMultiplier = Animator.StringToHash("animSpeedMultiplier");
        public static readonly int Open = Animator.StringToHash("open");
        public static readonly int IsOpen = Animator.StringToHash("isOpen");
        public static readonly int IsEnemy = Animator.StringToHash("isEnemy");
        public static readonly int IsTalk = Animator.StringToHash("isTalk");
        public static readonly int IsGreeting = Animator.StringToHash("isGreeting");
        public static readonly int IsApplause = Animator.StringToHash("isApplause");
        public static readonly int IsSwim = Animator.StringToHash("isSwim");
        public static readonly int Jump = Animator.StringToHash("jump");
        /// <summary>Trigger that opens the final portal (<c>FinalZone</c>).</summary>
        public static readonly int Start = Animator.StringToHash("start");

        /// <summary>Animator state name, used with <see cref="Animator.Play(int, int, float)"/>.</summary>
        public static readonly int WalkState = Animator.StringToHash("Walk");
    }
}
