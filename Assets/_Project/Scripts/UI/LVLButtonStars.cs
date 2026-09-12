using UnityEngine.UI;
using UnityEngine;
using Zenject;

using DreamVille.Persistence;

namespace DreamVille.UI
{
    public class LVLButtonStars : MonoBehaviour
    {
        [SerializeField] private string _levelId;
        [SerializeField] private Image[] _stars;

        private Button _button;
        private IProgressRepository _progress;

        [Inject]
        private void Construct(IProgressRepository progress)
        {
            _progress = progress;
        }

        private void Start()
        {
            _button = GetComponent<Button>();

            LoadStars();
            UpdateButtonState();
        }

        public void LoadStars()
        {
            int stars = _progress.GetStars(_levelId);

            for (int i = 0; i < _stars.Length; i++)
                _stars[i].gameObject.SetActive(i < stars);
        }

        private void UpdateButtonState()
        {
            // Level 1 is always unlocked; every other level opens once it has been completed.
            if (_levelId == "1")
            {
                _button.interactable = true;
                return;
            }

            _button.interactable = _progress.IsCompleted(_levelId);
        }
    }
}
