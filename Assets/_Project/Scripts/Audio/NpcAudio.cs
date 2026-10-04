using UnityEngine.Playables;
using UnityEngine;

namespace DreamVille.Audio
{
    /// <summary>Plays the NPC's Timeline (PlayableDirector) when the player enters the trigger.</summary>
    public class NpcAudio : MonoBehaviour
    {
        private PlayableDirector pd;

        void Start()
        {
            pd = GetComponent<PlayableDirector>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                pd.Play();
            }
        }
    }
}
