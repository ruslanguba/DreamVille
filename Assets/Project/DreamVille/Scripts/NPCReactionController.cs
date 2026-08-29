using System.Collections;
using UnityEngine;

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
        _npcAnimationHandler.SetIdleAnim();
    }

    private void PlayTalkSound()
    {
        int rnd = Random.Range(0, _mumbleClips.Length);
        _audio.PlayOneShot(_mumbleClips[rnd]);
    }

    public void SetGreeting()
    {
        _npcAnimationHandler.SetGreetingAnim();
    }

    public void SetApplause()
    {
        StartCoroutine(StartApplauseAndTalk());
    }

    public void SetIdle()
    {
        _audio.Stop();
        _npcAnimationHandler?.SetIdleAnim();
    }

    private IEnumerator StartApplauseAndTalk()
    {
        _npcAnimationHandler.SetisApplauseAnim();
        yield return new WaitForSeconds(3);
        PlayTalkSound();
        _npcAnimationHandler.SetTalkAnim();
    }
}
