using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

using DreamVille.Core.Async;

namespace DreamVille.Gameplay.Npc
{
    /// <summary>
    /// Drives an NPC's greeting/applause/talk/idle reactions to level events.
    /// The authored start pose belongs to <see cref="NpcAnimationHandler"/>; this component
    /// only changes state when a reaction is requested, and every new reaction supersedes a pending applause.
    /// </summary>
    [RequireComponent(typeof(NpcAnimationHandler))]
    public sealed class NpcReactionController : MonoBehaviour, INpcReactions
    {
        private const float ApplauseDuration = 3f;

        [SerializeField] private AudioClip[] _mumbleClips;

        private NpcAnimationHandler _npcAnimationHandler;
        private AudioSource _audio;
        private CancellationTokenSource _applauseCts;

        private void Awake()
        {
            _npcAnimationHandler = GetComponent<NpcAnimationHandler>();

            // Sound is optional: some NPCs stand without an AudioSource.
            _audio = GetComponent<AudioSource>();
        }

        private void OnDestroy()
        {
            CancelApplause();
        }

        public void SetGreeting()
        {
            CancelApplause();
            _npcAnimationHandler.SetState(NpcState.Greeting);
        }

        public void SetTalk()
        {
            CancelApplause();
            PlayTalkSound();
            _npcAnimationHandler.SetState(NpcState.Talk);
        }

        /// <summary>Mumbles and applauds at once, then falls into talking after <see cref="ApplauseDuration"/> seconds of game time.</summary>
        public void SetApplause()
        {
            if (!isActiveAndEnabled)
                return;

            CancelApplause();
            _applauseCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            ApplauseThenTalkAsync(_applauseCts.Token).Forget();
        }

        public void SetIdle()
        {
            CancelApplause();

            if (_audio != null)
                _audio.Stop();

            _npcAnimationHandler.SetState(NpcState.Idle);
        }

        private void CancelApplause() => Cancellation.CancelAndDispose(ref _applauseCts);

        private async UniTaskVoid ApplauseThenTalkAsync(CancellationToken token)
        {
            try
            {
                PlayTalkSound();
                _npcAnimationHandler.SetState(NpcState.Applause);
                await GameTime.DelayAsync(ApplauseDuration, token);

                _npcAnimationHandler.SetState(NpcState.Talk);
            }
            catch (OperationCanceledException)
            {
                // Superseded by a new reaction or the NPC was destroyed; nothing to do.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        private void PlayTalkSound()
        {
            if (_audio == null || _mumbleClips == null || _mumbleClips.Length == 0)
                return;

            _audio.PlayOneShot(_mumbleClips[Random.Range(0, _mumbleClips.Length)]);
        }
    }
}
