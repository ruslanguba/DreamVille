using System;
using UnityEngine;

namespace DreamVille.Gameplay
{
    /// <summary>Receives the walk cycle's jump animation event and forwards it to code as <see cref="JumpFrameReached"/>.</summary>
    public sealed class AnimationEventListener : MonoBehaviour
    {
        public event Action JumpFrameReached;

        // Called by name from the animation events on Art/Max/tenka/Animations/Walk.anim; renaming it silently breaks them.
        public void AnimationAction()
        {
            JumpFrameReached?.Invoke();
        }
    }
}
