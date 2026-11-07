using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

using DreamVille.Core.Async;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>
    /// On the first interaction activates each target after its own delay, counted from that interaction
    /// (scaled time); later interactions do nothing.
    /// </summary>
    public sealed class ActivateObjectsEffect : MonoBehaviour, IInteractionEffect
    {
        [SerializeField] private TimedActivation[] _activations = Array.Empty<TimedActivation>();

        private bool _activated;

        private void Awake()
        {
            for (int i = 0; i < _activations.Length; i++)
            {
                if (_activations[i].Target == null)
                    Debug.LogError($"[{nameof(ActivateObjectsEffect)}] Activation {i} has no target.", this);
            }
        }

        public void Apply(in InteractionContext context)
        {
            if (_activated)
                return;

            _activated = true;
            CancellationToken token = this.GetCancellationTokenOnDestroy();
            foreach (TimedActivation activation in _activations)
                ActivateAfterDelayAsync(activation, token).Forget();
        }

        private async UniTaskVoid ActivateAfterDelayAsync(TimedActivation activation, CancellationToken token)
        {
            try
            {
                if (activation.Delay > 0f)
                    await GameTime.DelayAsync(activation.Delay, token);

                activation.Target.SetActive(true);
            }
            catch (OperationCanceledException)
            {
                // Destroyed (or the scene unloaded) before the delay elapsed.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        /// <summary>A GameObject to activate and how long after the first interaction to do it.</summary>
        [Serializable]
        public struct TimedActivation
        {
            [SerializeField] private GameObject _target;
            [SerializeField, Min(0f)] private float _delay;

            public GameObject Target => _target;

            /// <summary>Seconds of scaled time between the first interaction and the activation.</summary>
            public float Delay => _delay;
        }
    }
}
