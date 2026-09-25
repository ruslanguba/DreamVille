using UnityEngine;

using DreamVille.Gameplay.Interactables;

namespace DreamVille.Tests.Support
{
    /// <summary>Test-only effect component, so a host can find it with <c>GetComponents</c>; counts its calls.</summary>
    public sealed class RecordingEffectBehaviour : MonoBehaviour, IInteractionEffect
    {
        public int CallCount { get; private set; }

        public void Apply(in InteractionContext context) => CallCount++;
    }
}
