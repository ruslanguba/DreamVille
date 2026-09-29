using UnityEngine;
using Zenject;

namespace DreamVille.App
{
    /// <summary>
    /// Caps the frame rate once for the whole application. Bound on the ProjectContext so the cap
    /// holds from the first scene, whichever scene Play Mode starts in.
    /// </summary>
    public sealed class FrameRateLimiter : IInitializable
    {
        public const int TargetFrameRate = 60;

        public void Initialize()
        {
            Application.targetFrameRate = TargetFrameRate;
        }
    }
}
