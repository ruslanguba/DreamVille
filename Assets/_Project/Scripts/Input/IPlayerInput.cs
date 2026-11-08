using System;
using UnityEngine;

namespace DreamVille.Input
{
    /// <summary>
    /// Player gameplay input: values to poll every frame (move, aim held) and events for presses. Camera look is not
    /// here: Cinemachine's input axis controller reads the Look action itself.
    /// </summary>
    public interface IPlayerInput
    {
        Vector2 Move { get; }

        bool IsAimHeld { get; }

        event Action JumpPressed;
        event Action FirePressed;

        /// <summary>Stops the pillow currently in flight.</summary>
        event Action StopPressed;

        /// <summary>Throws the next pillow, or recalls the last thrown one.</summary>
        event Action ThrowPressed;

        event Action<float> Scrolled;
    }
}
