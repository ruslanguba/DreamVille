using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

using DreamVille.Core.Async;

namespace DreamVille.Gameplay.NPC
{
    /// <summary>
    /// An ambient queue of NPCs: the front one jumps out, lands at the back slot, and everybody walks one slot forward.
    /// The slots are the members' positions when the scene loads. Runs while the component is enabled.
    /// </summary>
    public sealed class NpcQueueManager : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("jumpers")]
        [Tooltip("Queue members, front first.")]
        private List<NPCMover> _jumpers;

        [SerializeField, Min(0f), FormerlySerializedAs("moveDuration")]
        [Tooltip("Seconds of game time everybody walks to the next slot.")]
        private float _moveDuration = 1f;

        [SerializeField, Min(0f), FormerlySerializedAs("delayBetweenJumps")]
        [Tooltip("Seconds of game time between the jump and the walk.")]
        private float _delayBetweenJumps = 1.5f;

        private Vector3[] _slots;
        private RotatingQueue<NPCMover> _queue;
        private UniTask[] _moves;
        private CancellationTokenSource _loopCts;
        private bool _started;

        private void Awake()
        {
            if (!HasValidJumpers())
            {
                Debug.LogError($"[{nameof(NpcQueueManager)}] '{name}' needs at least one jumper and no empty slots.", this);
                enabled = false;
                return;
            }

            _slots = new Vector3[_jumpers.Count];
            for (int i = 0; i < _slots.Length; i++)
                _slots[i] = _jumpers[i].transform.position;

            _queue = new RotatingQueue<NPCMover>(_jumpers);
            _moves = new UniTask[_queue.Count];
        }

        private void Start()
        {
            // Not in OnEnable: the first step jumps at once, and movers on other objects must have run Awake.
            _started = true;
            StartLoop();
        }

        private void OnEnable()
        {
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
                    NPCMover jumper = _queue.Front;
                    jumper.JumpForward();
                    await GameTime.DelayAsync(_delayBetweenJumps, token);

                    // Re-queue first, then move everybody, the re-queued jumper included: its "move" holds it
                    // at the back slot while its no longer kinematic body settles.
                    _queue.RotateFrontToBack();
                    jumper.transform.position = _slots[_slots.Length - 1];
                    await MoveEveryoneToSlotsAsync(token);
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

        private UniTask MoveEveryoneToSlotsAsync(CancellationToken token)
        {
            for (int i = 0; i < _queue.Count; i++)
                _moves[i] = _queue[i].MoveToAsync(_slots[i], _moveDuration, token);

            return UniTask.WhenAll(_moves);
        }

        private bool HasValidJumpers()
        {
            if (_jumpers == null || _jumpers.Count == 0)
                return false;

            foreach (NPCMover jumper in _jumpers)
            {
                if (jumper == null)
                    return false;
            }

            return true;
        }
    }
}
