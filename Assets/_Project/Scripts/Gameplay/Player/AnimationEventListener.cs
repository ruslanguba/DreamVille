using System;
using UnityEngine;

namespace DreamVille.Gameplay.Player
{
    /// <summary>Receives the jump animation event and forwards it to code as OnAnimationJump.</summary>
    public class AnimationEventListener : MonoBehaviour
    {
        public event Action OnAnimationJump;

        public void AnimationAction()
        {
            OnAnimationJump?.Invoke();
        }
    }
}
