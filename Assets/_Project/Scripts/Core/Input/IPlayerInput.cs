using System;
using UnityEngine;

namespace DreamVille.Core.Input
{
    public interface IPlayerInput
    {
        // Pull
        Vector2 Move { get; }
        Vector2 Look { get; }

        bool IsFireHeld { get; }
        bool IsAimHeld { get; }

        event Action Jump;
        event Action FirePressed;
        event Action FireReleased;
        event Action SecondaryFirePressed;
        event Action SecondaryFireReleased;
        event Action Recall;
        event Action Throw;
        event Action Interact;
        event Action<float> Scroll;
    }
}
