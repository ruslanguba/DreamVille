using System;
using UnityEngine;

using DreamVille.Input;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary>Hand-written double for <see cref="IPlayerInput"/> with settable values and raise helpers for every event.</summary>
    public sealed class FakePlayerInput : IPlayerInput
    {
        public Vector2 Move { get; set; }

        public Vector2 Look { get; set; }

        public bool IsFireHeld { get; set; }

        public bool IsAimHeld { get; set; }

        public event Action Jump;
        public event Action FirePressed;
        public event Action FireReleased;
        public event Action SecondaryFirePressed;
        public event Action SecondaryFireReleased;
        public event Action Stop;
        public event Action Throw;
        public event Action Interact;
        public event Action<float> Scroll;

        public void RaiseJump() => Jump?.Invoke();

        public void RaiseFirePressed() => FirePressed?.Invoke();

        public void RaiseFireReleased() => FireReleased?.Invoke();

        public void RaiseSecondaryFirePressed() => SecondaryFirePressed?.Invoke();

        public void RaiseSecondaryFireReleased() => SecondaryFireReleased?.Invoke();

        public void RaiseStop() => Stop?.Invoke();

        public void RaiseThrow() => Throw?.Invoke();

        public void RaiseInteract() => Interact?.Invoke();

        public void RaiseScroll(float delta) => Scroll?.Invoke(delta);
    }
}
