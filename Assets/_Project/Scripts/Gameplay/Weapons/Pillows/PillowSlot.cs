using System;
using UnityEngine;

namespace DreamVille.Gameplay.Weapons.Pillows
{
    /// <summary>
    /// State machine of one pillow: Stowed → Flying → (Placed) → Returning → Stowed.
    /// Illegal transitions return false and touch nothing.
    /// </summary>
    public sealed class PillowSlot
    {
        private readonly IPillowBody _body;

        /// <summary>Stows <paramref name="body"/> so every slot starts in hand.</summary>
        public PillowSlot(IPillowBody body)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _body.Stow();
            State = PillowState.Stowed;
        }

        public PillowState State { get; private set; }

        /// <summary>Stowed → Flying: launches the body from <paramref name="launch"/> with <paramref name="impulse"/>.</summary>
        public bool TryThrow(Pose launch, Vector3 impulse)
        {
            if (State != PillowState.Stowed)
                return false;

            _body.Launch(launch.position, launch.rotation, impulse);
            State = PillowState.Flying;
            return true;
        }

        /// <summary>Flying → Placed: freezes the body into a platform.</summary>
        public bool TryFreeze()
        {
            if (State != PillowState.Flying)
                return false;

            _body.Freeze();
            State = PillowState.Placed;
            return true;
        }

        /// <summary>
        /// Flying or Placed → Returning. A body farther than <paramref name="snapDistance"/> from
        /// <paramref name="returnPoint"/> first jumps onto that radius, so a recall never takes long.
        /// </summary>
        public bool TryBeginReturn(Vector3 returnPoint, float snapDistance)
        {
            if (State != PillowState.Flying && State != PillowState.Placed)
                return false;

            _body.BeginReturn(ClampToRadius(_body.Position, returnPoint, snapDistance));
            State = PillowState.Returning;
            return true;
        }

        /// <summary>
        /// Moves a returning body up to <paramref name="maxDistanceDelta"/> towards <paramref name="returnPoint"/>.
        /// Within <paramref name="arrivalThreshold"/> it snaps onto the point and is stowed.
        /// </summary>
        /// <returns>True when the body arrived and the slot is stowed again.</returns>
        public bool StepReturn(Vector3 returnPoint, float maxDistanceDelta, float arrivalThreshold)
        {
            if (State != PillowState.Returning)
                return false;

            Vector3 next = Vector3.MoveTowards(_body.Position, returnPoint, maxDistanceDelta);
            if ((next - returnPoint).sqrMagnitude > arrivalThreshold * arrivalThreshold)
            {
                _body.MoveTo(next);
                return false;
            }

            _body.MoveTo(returnPoint);
            Stow();
            return true;
        }

        /// <summary>Returning → Stowed at once; the cancellation path, so a slot is never stranded mid-return.</summary>
        public bool CompleteReturn()
        {
            if (State != PillowState.Returning)
                return false;

            Stow();
            return true;
        }

        private void Stow()
        {
            _body.Stow();
            State = PillowState.Stowed;
        }

        private static Vector3 ClampToRadius(Vector3 position, Vector3 center, float radius)
        {
            Vector3 offset = position - center;
            return offset.sqrMagnitude > radius * radius
                ? center + offset.normalized * radius
                : position;
        }
    }
}
