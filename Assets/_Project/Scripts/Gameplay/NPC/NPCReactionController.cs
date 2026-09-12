using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DreamVille.Gameplay.NPC
{
    /// <summary>
    /// Drives an NPC's greeting/applause/talk/idle reactions to level events.
    /// The authored start pose belongs to <see cref="NPCAnimationHandler"/>; this component
    /// only changes state when a reaction is requested.
    /// </summary>
    [RequireComponent(typeof(NPCAnimationHandler))]
    public class NPCReactionController : MonoBehaviour, INpcReactions
    {
        private const float ApplauseDuration = 3f;

        [SerializeField] private AudioClip[] _mumbleClips;

        private NPCAnimationHandler _npcAnimationHandler;
        private AudioSource _audio;
        private CancellationTokenSource _applauseCts;

        private void Awake()
        {
            _npcAnimationHandler = GetComponent<NPCAnimationHandler>();

            // Sound is optional: some NPCs stand without an AudioSource.
            _audio = GetComponent<AudioSource>();
        }

        private void OnDestroy()
        {
            CancelApplause();
        }

        public void SetGreeting() => _npcAnimationHandler.SetGreetingAnim();

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

            _npcAnimationHandler.SetIdleAnim();
        }

        private void CancelApplause()
        {
            if (_applauseCts == null)
                return;

            _applauseCts.Cancel();
            _applauseCts.Dispose();
            _applauseCts = null;
        }

        private async UniTaskVoid ApplauseThenTalkAsync(CancellationToken token)
        {
            try
            {
                _npcAnimationHandler.SetApplauseAnim();
                await UniTask.Delay(TimeSpan.FromSeconds(ApplauseDuration), cancellationToken: token);

                PlayTalkSound();
                _npcAnimationHandler.SetTalkAnim();
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
