using UnityEngine;

using DreamVille.Input;

namespace DreamVille.UI
{
    public class CanvasTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject _text;

        private void Start()
        {
            _text.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.TryGetComponent(out InputReader inputReader))
            {
                _text.SetActive(true);
                _text.transform.LookAt(other.transform.position);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            _text.SetActive(false);
        }
    }
}
