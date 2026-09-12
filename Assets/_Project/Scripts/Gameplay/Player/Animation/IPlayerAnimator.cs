using UnityEngine;

namespace DreamVille.Gameplay.Player.Animation
{
    /// <summary>Presentation seam for the player's animator, so gameplay logic never touches Animator parameters directly.</summary>
    public interface IPlayerAnimator
    {
        /// <summary>Re-targets the driver at a different skin's Animator (see SkinHandler).</summary>
        void SetAnimator(Animator animator);

        void SetLocomotion(float speed, in LocomotionState state);

        void TriggerJump();
    }
}
