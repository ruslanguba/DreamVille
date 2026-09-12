using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

using DreamVille.Core.Input;

namespace DreamVille.Gameplay.Weapons.Pillow
{
    /// <summary>
    /// Throws pillows from a pre-assigned pool and recalls or freezes them mid-flight.
    /// One key drives throwing and recalling: while a pillow is in flight the key recalls it;
    /// otherwise it throws the next free pillow, and once every pillow is placed it recalls
    /// the most recently placed one. Stop freezes the pillow in flight so it becomes a static
    /// platform; a frozen pillow stays "placed" and is recalled when no free pillow is left.
    /// </summary>
    public class PillowThrowHandler : MonoBehaviour
    {
        /// <summary>Raised after a pillow is thrown.</summary>
        public event Action OnThrow;

        /// <summary>Raised after a recalled pillow arrives back at the spawn point.</summary>
        public event Action OnRecall;

        /// <summary>Raised once during initialization with the number of available pillow slots.</summary>
        public event Action<int> OnPillowAdded;

        private const float ArrivalThreshold = 0.1f;

        [Header("References")]
        [SerializeField] private Transform _spawnPoint;
        [SerializeField] private Transform _character;
        [SerializeField] private List<GameObject> _pillows;

        [Header("Settings")]
        [SerializeField] private float _throwForce = 10f;
        [SerializeField] private float _recallSpeed = 12f;
        [SerializeField] private float _recallSnapDistance = 10f;

        private readonly List<PillowSlot> _slots = new();
        private readonly Stack<PillowSlot> _thrownPillows = new();

        private IPlayerInput _input;
        private bool _isSubscribed;
        private bool _isRecalling;
        private Rigidbody _flyingPillow;

        private readonly struct PillowSlot
        {
            public readonly GameObject GameObject;
            public readonly Rigidbody Rigidbody;

            public PillowSlot(GameObject gameObject, Rigidbody rigidbody)
            {
                GameObject = gameObject;
                Rigidbody = rigidbody;
            }
        }

        /// <summary>Resolves input, prepares the pillow pool and subscribes to input events.</summary>
        public void Initialize()
        {
            _input = GetComponent<IPlayerInput>();

            for (int i = 0; i < _pillows.Count; i++)
            {
                GameObject pillow = _pillows[i];
                if (pillow == null)
                {
                    Debug.LogError($"[{nameof(PillowThrowHandler)}] Pillow slot {i} is not assigned.", this);
                    continue;
                }

                var rigidbody = pillow.GetComponent<Rigidbody>();
                if (rigidbody == null)
                {
                    Debug.LogError($"[{nameof(PillowThrowHandler)}] Pillow '{pillow.name}' has no Rigidbody.", pillow);
                    continue;
                }

                pillow.transform.parent = null;

                // Velocities must be cleared before the body goes kinematic; Unity 6
                // logs an error if a kinematic Rigidbody still carries velocity.
                rigidbody.linearVelocity = Vector3.zero;
                rigidbody.angularVelocity = Vector3.zero;
                rigidbody.isKinematic = true;

                pillow.SetActive(false);
                _slots.Add(new PillowSlot(pillow, rigidbody));
            }

            Subscribe();
            OnPillowAdded?.Invoke(_slots.Count);
        }

        private void OnEnable()
        {
            if (_input != null)
                Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_isSubscribed || _input == null)
                return;

            _input.Throw += HandleThrowOrRecall;
            _input.Stop += HandleStop;
            _isSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isSubscribed)
                return;

            _input.Throw -= HandleThrowOrRecall;
            _input.Stop -= HandleStop;
            _isSubscribed = false;
        }

        private void HandleThrowOrRecall()
        {
            if (_isRecalling)
                return;

            // The pillow in flight is always the most recently thrown one, i.e. the top of the stack.
            if (_flyingPillow != null && _thrownPillows.Count > 0)
            {
                Recall(_thrownPillows.Pop());
                return;
            }

            for (int i = 0; i < _slots.Count; i++)
            {
                if (!_slots[i].GameObject.activeInHierarchy)
                {
                    Throw(_slots[i]);
                    return;
                }
            }

            // Every pillow is placed as a platform: take back the most recent one.
            if (_thrownPillows.Count > 0)
                Recall(_thrownPillows.Pop());
        }

        private void HandleStop()
        {
            if (_flyingPillow == null)
                return;

            _flyingPillow.linearVelocity = Vector3.zero;
            _flyingPillow.angularVelocity = Vector3.zero;
            _flyingPillow.isKinematic = true;
            _flyingPillow = null;
        }

        private void Throw(PillowSlot slot)
        {
            Rigidbody rigidbody = slot.Rigidbody;
            GameObject pillow = slot.GameObject;

            pillow.transform.position = _spawnPoint.position;
            pillow.transform.rotation = _character.rotation;
            pillow.SetActive(true);

            rigidbody.isKinematic = false;
            rigidbody.detectCollisions = true;
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.AddForce(_character.forward * _throwForce, ForceMode.Impulse);

            _flyingPillow = rigidbody;
            _thrownPillows.Push(slot);
            OnThrow?.Invoke();
        }

        private void Recall(PillowSlot slot)
        {
            if (_flyingPillow == slot.Rigidbody)
                _flyingPillow = null;

            RecallAsync(slot.Rigidbody, this.GetCancellationTokenOnDestroy()).Forget();
        }

        private async UniTaskVoid RecallAsync(Rigidbody pillow, CancellationToken token)
        {
            _isRecalling = true;

            try
            {
                pillow.linearVelocity = Vector3.zero;
                pillow.angularVelocity = Vector3.zero;
                pillow.isKinematic = true;
                pillow.detectCollisions = false;

                float distance = Vector3.Distance(pillow.position, _spawnPoint.position);
                if (distance > _recallSnapDistance)
                {
                    Vector3 direction = (_spawnPoint.position - pillow.position).normalized;
                    pillow.position = _spawnPoint.position - direction * _recallSnapDistance;
                }

                while (Vector3.Distance(pillow.position, _spawnPoint.position) > ArrivalThreshold)
                {
                    pillow.position = Vector3.MoveTowards(pillow.position, _spawnPoint.position, _recallSpeed * Time.deltaTime);
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }

                pillow.position = _spawnPoint.position;
                pillow.gameObject.SetActive(false);
                OnRecall?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // The object was destroyed while the pillow was returning; nothing to clean up.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
            finally
            {
                _isRecalling = false;
            }
        }
    }
}
