using System.Collections.Generic;
using System.Collections;
using UnityEngine;

using DreamVille.Gameplay.Player;

namespace DreamVille.UI
{
    public class Dialog : MonoBehaviour
    {
        [SerializeField] private GameObject _text;     

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.TryGetComponent(out PlayerContext playerContext))
            {
                _text.SetActive(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {        
            if (other.gameObject.TryGetComponent(out PlayerContext playerContext))
            {
                _text.SetActive(false);
            }
        }
    }
}
