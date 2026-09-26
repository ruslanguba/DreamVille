using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

using DreamVille.Core.Async;

namespace DreamVille.Gameplay.NPC
{
    /// <summary>
    /// Ambient dialogue: the first speaker starts talking, then after every switch interval (game time)
    /// each speaker flips between talking and listening. Runs while the component is enabled.
    /// </summary>
    public sealed class NPCDialogueHandler : MonoBehaviour
    {
        [SerializeField, Min(0f), FormerlySerializedAs("_timer")]
        [Tooltip("Seconds of game time between two turns.")]
        private float _switchInterval = 5f;

        [SerializeField, FormerlySerializedAs("_handler")]
        private NPCHandler[] _speakers;

        private CancellationTokenSource _loopCts;
        private bool _started;

        private void Awake()
        {
            if (!HasValidSpeakers())
            {
                Debug.LogError($"[{nameof(NPCDialogueHandler)}] '{name}' needs at least one speaker and no empty slots.", this);
                enabled = false;
            }
        }

        private void Start()
        {
            for (int i = 0; i < _speakers.Length; i++)
                _speakers[i].SetTalking(i == 0);

            _started = true;
            StartLoop();
        }

        private void OnEnable()
        {
            // The first run starts in Start, after every speaker has run Awake.
            if (_started)
                StartLoop();
        }

        private void OnDisable()
        {
            Cancellation.CancelAndDispose(ref _loopCts);
        }

        private void StartLoop()
        {
            Cancellation.CancelAndDispose(ref _loopCts);
            _loopCts = new CancellationTokenSource();
            RunAsync(_loopCts.Token).Forget();
        }

        private async UniTaskVoid RunAsync(CancellationToken token)
        {
            try
            {
                while (true)
                {
                    await GameTime.DelayAsync(_switchInterval, token);

                    foreach (NPCHandler speaker in _speakers)
                        speaker.SwitchTalking();
                }
            }
            catch (OperationCanceledException)
            {
                // Disabled or destroyed; the loop restarts on the next OnEnable.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private bool HasValidSpeakers()
        {
            if (_speakers == null || _speakers.Length == 0)
                return false;

            foreach (NPCHandler speaker in _speakers)
            {
                if (speaker == null)
                    return false;
            }

            return true;
        }
    }
}
