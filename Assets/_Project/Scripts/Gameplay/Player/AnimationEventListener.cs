using System;
using UnityEngine;

namespace DreamVille.Gameplay.Player
{
    public class AnimationEventListener : MonoBehaviour
    {
        public event Action OnAnimationJump;

        public void AnimationAction()
        {
            OnAnimationJump?.Invoke();
        }
    }
}
