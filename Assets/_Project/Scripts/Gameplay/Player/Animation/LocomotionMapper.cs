using UnityEngine;

namespace DreamVille.Gameplay.Player.Animation
{
    /// <summary>Pure mapping of stick input to the animator locomotion codes.</summary>
    public static class LocomotionMapper
    {
        public const float DeadZone = 0.05f;

        public static LocomotionState Map(Vector2 moveInput)
        {
            bool isMoving = moveInput.magnitude > DeadZone;
            if (!isMoving)
                return new LocomotionState(false, MoveDirection.None, 1f);

            bool forward = moveInput.y > DeadZone;
            bool backward = moveInput.y < -DeadZone;
            bool right = moveInput.x > DeadZone;
            bool left = moveInput.x < -DeadZone;

            MoveDirection direction;
            float multiplier;

            if (forward)
            {
                direction = right ? MoveDirection.StrafeRight : left ? MoveDirection.StrafeLeft : MoveDirection.Forward;
                multiplier = 1f;
            }
            else if (backward)
            {
                // Backward mirrors the strafe: backward + right selects the left strafe clip, and vice versa.
                direction = right ? MoveDirection.StrafeLeft : left ? MoveDirection.StrafeRight : MoveDirection.Forward;
                multiplier = -1f;
            }
            else
            {
                direction = right ? MoveDirection.StrafeRight : left ? MoveDirection.StrafeLeft : MoveDirection.None;
                multiplier = 1f;
            }

            return new LocomotionState(true, direction, multiplier);
        }
    }
}
