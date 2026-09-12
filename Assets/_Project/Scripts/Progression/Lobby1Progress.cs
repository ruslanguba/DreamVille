using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

using DreamVille.Dialogue;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.NPC;

namespace DreamVille.Progression
{
    /// <summary>
    /// Scripted two-quest lobby sequence: kill the first enemy group to open a door and
    /// trigger a greeting dialogue, then kill the second group to open the final door.
    /// </summary>
    public class Lobby1Progress : MonoBehaviour
    {
        [FormerlySerializedAs("first_Enemies")]
        [SerializeField] private EnemyHealth[] _firstEnemies;
        [FormerlySerializedAs("second_Enemies")]
        [SerializeField] private EnemyHealth[] _secondEnemies;

        [FormerlySerializedAs("first_Dilogue")]
        [SerializeField] private DialogueTrigger _firstDialogue;
        [FormerlySerializedAs("second_Dilogue")]
        [SerializeField] private DialogueTrigger _secondDialogue;

        [FormerlySerializedAs("first_door")]
        [SerializeField] private Animator _firstDoor;
        [FormerlySerializedAs("second_door")]
        [SerializeField] private Animator _secondDoor;

        [FormerlySerializedAs("audioSource")]
        [SerializeField] private AudioSource _audioSource;
        [FormerlySerializedAs("doorOpenSound")]
        [SerializeField] private AudioClip _doorOpenSound;
        [FormerlySerializedAs("mombleSound")]
        [SerializeField] private AudioClip[] _mumbleSounds;
        [FormerlySerializedAs("npcAnimationHandler")]
        [SerializeField] private NPCAnimationHandler _npcAnimationHandler;

        private int _firstQuestKills;
        private int _secondQuestKills;
        private bool _isSubscribed;

        private void Start()
        {
            _firstQuestKills = 0;
            _secondQuestKills = 0;
            _secondDialogue.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (_isSubscribed)
                return;

            foreach (EnemyHealth enemy in _firstEnemies)
            {
                enemy.OnDeath += CalculateFirstQuestProgress;
            }

            foreach (EnemyHealth enemy in _secondEnemies)
            {
                enemy.OnDeath += CalculateSecondQuestProgress;
            }

            _firstDialogue.DialogueStarted += SetTalkAnim;
            _firstDialogue.DialogueEnded += OpenFirstDoor;

            _secondDialogue.DialogueStarted += SetApplauseAnim;
            _secondDialogue.DialogueEnded += StopAudio;

            _isSubscribed = true;
        }

        private void OnDisable()
        {
            if (!_isSubscribed)
                return;

            foreach (EnemyHealth enemy in _firstEnemies)
            {
                enemy.OnDeath -= CalculateFirstQuestProgress;
            }

            foreach (EnemyHealth enemy in _secondEnemies)
            {
                enemy.OnDeath -= CalculateSecondQuestProgress;
            }

            _firstDialogue.DialogueStarted -= SetTalkAnim;
            _firstDialogue.DialogueEnded -= OpenFirstDoor;

            _secondDialogue.DialogueStarted -= SetApplauseAnim;
            _secondDialogue.DialogueEnded -= StopAudio;

            _isSubscribed = false;
        }

        private void CalculateFirstQuestProgress(EnemyHealth enemy, int collisions)
        {
            _firstQuestKills++;
            if (_firstQuestKills >= _firstEnemies.Length)
            {
                _secondDialogue.gameObject.SetActive(true);
                _npcAnimationHandler.SetGreetingAnim();
            }
        }

        private void CalculateSecondQuestProgress(EnemyHealth enemy, int collisions)
        {
            _secondQuestKills++;
            if (_secondQuestKills >= _secondEnemies.Length)
            {
                _npcAnimationHandler.SetGreetingAnim();
                OpenSecondDoor();
            }
        }

        private void OpenFirstDoor()
        {
            _audioSource.Stop();
            _audioSource.transform.position = _firstDoor.transform.position;
            _audioSource.PlayOneShot(_doorOpenSound);
            _firstDoor.SetTrigger("open");
            _npcAnimationHandler.SetIdleAnim();
        }

        private void OpenSecondDoor()
        {
            _audioSource.Stop();
            _audioSource.transform.position = _secondDoor.transform.position;
            _audioSource.PlayOneShot(_doorOpenSound);
            _secondDoor.SetTrigger("open");
            _npcAnimationHandler.SetIdleAnim();
        }

        private void SetTalkAnim()
        {
            int rnd = Random.Range(0, _mumbleSounds.Length);
            _audioSource.PlayOneShot(_mumbleSounds[rnd]);
            _npcAnimationHandler.SetTalkAnim();
        }

        private void SetApplauseAnim()
        {
            int rnd = Random.Range(0, _mumbleSounds.Length);
            _audioSource.PlayOneShot(_mumbleSounds[rnd]);
            StartCoroutine(StartApplauseAndTalk());
        }

        private IEnumerator StartApplauseAndTalk()
        {
            _npcAnimationHandler.SetApplauseAnim();
            yield return new WaitForSeconds(3);
            _npcAnimationHandler.SetTalkAnim();
        }

        private void StopAudio()
        {
            _audioSource.Stop();
        }
    }
}
