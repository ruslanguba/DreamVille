using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

using DreamVille.Dialogue;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.NPC;
using DreamVille.Gameplay.Weapons;

namespace DreamVille.Progression
{
    public class Lobby1Progress : MonoBehaviour
    {
        [SerializeField] private EnemyHealth[] first_Enemies;
        [SerializeField] private EnemyHealth[] second_Enemies;

        [FormerlySerializedAs("first_Dilogue")]
        [SerializeField] private DialogueTrigger _firstDialogue;
        [FormerlySerializedAs("second_Dilogue")]
        [SerializeField] private DialogueTrigger _secondDialogue;

        [SerializeField] private Animator first_door;
        [SerializeField] private Animator second_door;

        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip doorOpenSound;
        [FormerlySerializedAs("mombleSound")]
        [SerializeField] private AudioClip[] _mumbleSounds;
        [SerializeField] private NPCAnimationHandler npcAnimationHandler;

        [SerializeField] private int _counter;

        private void Start()
        {
            _counter = 0;
            foreach (EnemyHealth enemy in first_Enemies)
            {
                enemy.OnDeath += CalculateFirstQuestProgress;
            }

            foreach (EnemyHealth enemy in second_Enemies)
            {
                enemy.OnDeath += CalculateSecondQuestProgress;
            }

            _firstDialogue.DialogueStarted += SetTalkAnim;
            _firstDialogue.DialogueEnded += OpenFirstDoor;

            _secondDialogue.DialogueStarted += SetApplauseAnim;
            _secondDialogue.DialogueEnded += StopAudio;

            _secondDialogue.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            _firstDialogue.DialogueStarted -= SetTalkAnim;
            _firstDialogue.DialogueEnded -= OpenFirstDoor;

            _secondDialogue.DialogueStarted -= SetApplauseAnim;
            _secondDialogue.DialogueEnded -= StopAudio;

        }
        private void CalculateFirstQuestProgress(EnemyHealth enemy, int collisions)
        {
            _counter++;
            if(_counter >= 2)
            {
                _secondDialogue.gameObject.SetActive(true);
                npcAnimationHandler.SetGreetingAnim();
                _counter = 0;
            }
        }
        private void CalculateSecondQuestProgress(EnemyHealth enemy, int collisions)
        {
            _counter++;
            if (_counter >= second_Enemies.Length)
            {
                npcAnimationHandler.SetGreetingAnim();
                OpenSecondDoor();
            }
        }

        private void OpenFirstDoor()
        {
            audioSource.Stop();
            audioSource.transform.position = first_door.transform.position;
            audioSource.PlayOneShot(doorOpenSound);
            first_door.SetTrigger("open");
            npcAnimationHandler.SetIdleAnim();
        }

        private void OpenSecondDoor()
        {
            audioSource.Stop();
            audioSource.transform.position = second_door.transform.position;
            audioSource.PlayOneShot(doorOpenSound);
            second_door.SetTrigger("open");
            npcAnimationHandler.SetIdleAnim();
        }

        private void SetTalkAnim()
        {
            int rnd = Random.Range(0, _mumbleSounds.Length);
            audioSource.PlayOneShot(_mumbleSounds[rnd]);
            npcAnimationHandler.SetTalkAnim();
        }

        private void SetApplauseAnim()
        {
            int rnd = Random.Range(0, _mumbleSounds.Length);
            audioSource.PlayOneShot(_mumbleSounds[rnd]);
            StartCoroutine(StartApplauseAndTalk());
        }

        private IEnumerator StartApplauseAndTalk()
        {
            npcAnimationHandler.SetApplauseAnim();
            yield return new WaitForSeconds(3);
            npcAnimationHandler.SetTalkAnim();
        }

        private void StopAudio()
        {
            audioSource.Stop();
        }
    }
}
