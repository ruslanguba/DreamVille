using System;
using UnityEngine;

public interface IPlayerInput
{
    // Pull
    Vector2 Move { get; }
    Vector2 Look { get; }

    bool IsFireHeld { get; }
    bool IsAimHeld { get; }

    // Push (одноразовые события)
    event Action Jump;
    event Action FirePressed;
    event Action FireReleased;
    event Action SecondaryFirePressed;
    event Action SecondaryFireReleased;
    event Action Recall;
    event Action Throw;
    event Action Interact;
    //event Action Pause;
    event Action<float> Scroll;
}