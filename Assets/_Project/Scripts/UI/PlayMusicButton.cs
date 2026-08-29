using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace DreamVille.UI
{
    public class PlayMusicButton : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _clickClip;

        public void ClickSound()
        {
            _audioSource.PlayOneShot(_clickClip);
        }
    }
}
