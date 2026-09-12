using System;
using UnityEngine;

using DreamVille.Input;

namespace DreamVille.Gameplay.Triggers
{
    public class HintActivatorTrigger : MonoBehaviour
    {
        public event Action<bool, Vector3> OnTriggerActivated;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out InputReader _))
            {
                OnTriggerActivated?.Invoke(true, other.transform.position);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.TryGetComponent(out InputReader _))
            {
                OnTriggerActivated?.Invoke(false, other.transform.position);
            }
        }
    }
}
