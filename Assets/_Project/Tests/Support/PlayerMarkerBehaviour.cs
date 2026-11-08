using UnityEngine;

using DreamVille.Core.Players;

namespace DreamVille.Tests.Support
{
    /// <summary>Test-only <see cref="IPlayerMarker"/> that stands for the player root.</summary>
    public sealed class PlayerMarkerBehaviour : MonoBehaviour, IPlayerMarker
    {
        public Transform Transform => transform;
    }
}
