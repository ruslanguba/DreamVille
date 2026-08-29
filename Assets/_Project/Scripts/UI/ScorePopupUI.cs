using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

namespace DreamVille.UI
{
    public class ScorePopupUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _addedScore;
        [SerializeField] private Image _addedScoreImage;

        private void Awake()
        {
            _addedScoreImage.gameObject.SetActive(false);
        }

        public void ShowScore(float score)
        {
            _addedScoreImage.gameObject.SetActive(true);
            _addedScore.text = score.ToString();
            StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            yield return new WaitForSeconds(3);
            _addedScoreImage.gameObject.SetActive(false);
        }
    }
}
