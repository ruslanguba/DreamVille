using System;

using DreamVille.Gameplay.Interactables;

namespace DreamVille.Tests.EditMode.Fakes
{
    /// <summary><see cref="IInteractionEffect"/> double that always throws, to check that a chain survives a failing effect.</summary>
    public sealed class ThrowingEffect : IInteractionEffect
    {
        public const string Message = "ThrowingEffect failed on purpose.";

        public void Apply(in InteractionContext context) => throw new InvalidOperationException(Message);
    }
}
