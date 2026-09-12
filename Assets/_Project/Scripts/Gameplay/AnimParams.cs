using UnityEngine;

namespace DreamVille.Gameplay
{
    /// <summary>Cached animator hashes. Parameter and state names live here only.</summary>
    public static class AnimParams
    {
        public static readonly int Dead = Animator.StringToHash("dead");
        public static readonly int IsMoving = Animator.StringToHash("isMoving");

        /// <summary>Animator state name, used with <see cref="Animator.Play(int, int, float)"/>.</summary>
        public static readonly int WalkState = Animator.StringToHash("Walk");
    }
}
