using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

using DreamVille.Core.Async;

namespace DreamVille.Gameplay.Npc
{
    /// <summary>
    /// Ambient dialogue: the first speaker starts talking, then after every switch interval (game time)
    /// each speaker flips between talking and listening. Runs while the component is enabled.
    /// </summary>
    public sealed class NpcDialogueHandler : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        [Tooltip("Seconds of game time between two turns.")]
        private float _switchInterval = 5f;

        [SerializeField]
        private NpcHandler[] _speakers;

        private CancellationTokenSource _loopCts;
        private bool _started;

        private void Awake()
        {
            if (!HasValidSpeakers())
            {
                Debug.LogError($"[{nameof(NpcDialogueHandler)}] '{name}' needs at least one speaker and no empty slots.", this);
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

                    foreach (NpcHandler speaker in _speakers)
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

            foreach (NpcHandler speaker in _speakers)
            {
                if (speaker == null)
                    return false;
            }

            return true;
        }
    }
}
