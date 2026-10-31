using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

using DreamVille.Core.Async;

namespace DreamVille.Gameplay.Npc
{
    /// <summary>A queue member's body: jumps out of the front slot and walks to a new slot.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class NpcMover : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Upward velocity change of a jump, m/s.")]
        private float _jumpForce = 5f;

        [SerializeField]
        [Tooltip("Forward velocity change of a jump, m/s.")]
        private float _forwardForce = 2f;

        private Rigidbody _body;
        private Animator _animator;
        private CancellationTokenSource _moveCts;

        private void Awake()
        {
            _body = GetComponent<Rigidbody>();
            _animator = GetComponent<Animator>();

            if (_animator == null)
            {
                Debug.LogError($"[{nameof(NpcMover)}] '{name}' has no Animator.", this);
                enabled = false;
            }
        }

        private void OnDestroy()
        {
            Cancellation.CancelAndDispose(ref _moveCts);
        }

        /// <summary>
        /// Lerps to <paramref name="target"/> over <paramref name="duration"/> seconds of game time with the run flag on.
        /// A newer move supersedes this one; cancellation (token, newer move, destroy) throws OperationCanceledException
        /// and leaves the NPC where it stopped.
        /// </summary>
        public async UniTask MoveToAsync(Vector3 target, float duration, CancellationToken token)
        {
            Cancellation.CancelAndDispose(ref _moveCts);
            CancellationTokenSource move = CancellationTokenSource.CreateLinkedTokenSource(token, this.GetCancellationTokenOnDestroy());
            _moveCts = move;

            try
            {
                Vector3 start = transform.position;
                _animator.SetBool(AnimParams.IsMoving, true);

                await Tween.RunAsync(duration, progress => transform.position = Vector3.Lerp(start, target, progress), move.Token);

                _animator.SetBool(AnimParams.IsMoving, false);
            }
            finally
            {
                // A superseding move has already disposed this source and replaced the field.
                if (_moveCts == move)
                    Cancellation.CancelAndDispose(ref _moveCts);
            }
        }

        /// <summary>Plays the jump and throws the body up and forward; the body stops being kinematic.</summary>
        public void JumpForward()
        {
            _animator.SetTrigger(AnimParams.Jump);

            _body.isKinematic = false;
            _body.AddForce(transform.up * _jumpForce + transform.forward * _forwardForce, ForceMode.VelocityChange);
        }
    }
}
