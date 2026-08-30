using System.Collections;
using UnityEngine;

namespace DreamVille.Gameplay.NPC
{
    public class NPCReactionController : MonoBehaviour
    {
        [SerializeField] private NPCAnimationHandler _npcAnimationHandler;
        [SerializeField] private AudioSource _audio;
        [SerializeField] private AudioClip[] _mumbleClips;

        private void Awake()
        {
            _npcAnimationHandler = GetComponent<NPCAnimationHandler>();
            _audio = GetComponent<AudioSource>();
        }

        private void Start()
        {
            _npcAnimationHandler?.SetIdleAnim();
        }

        // Звук у NPC опционален: часть персонажей стоит без AudioSource.
        private void PlayTalkSound()
        {
            if (_audio == null || _mumbleClips == null || _mumbleClips.Length == 0)
                return;

            _audio.PlayOneShot(_mumbleClips[Random.Range(0, _mumbleClips.Length)]);
        }

        public void SetGreeting() => _npcAnimationHandler?.SetGreetingAnim();

        public void SetApplause()
        {
            if (!isActiveAndEnabled)
                return;

            StartCoroutine(StartApplauseAndTalk());
        }

        public void SetIdle()
        {
            if (_audio != null)
                _audio.Stop();

            _npcAnimationHandler?.SetIdleAnim();
        }

        private IEnumerator StartApplauseAndTalk()
        {
            _npcAnimationHandler?.SetisApplauseAnim();
            yield return new WaitForSeconds(3);
            PlayTalkSound();
            _npcAnimationHandler?.SetTalkAnim();
        }
    }
}
