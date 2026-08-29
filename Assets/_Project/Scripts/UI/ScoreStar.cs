using System.Collections;
using UnityEngine.UI;
using UnityEngine;

namespace DreamVille.UI
{
    public class ScoreStar : MonoBehaviour
    {
        [SerializeField] private float _scoreToFill;
        [SerializeField] private float _popScale = 1.3f;
        [SerializeField] private float _popTime = 0.2f;
        [SerializeField] private ParticleSystem _particle;

        private Image _image;
        private float _currentScore;
        private Vector3 _originalScale;
        private Coroutine _popCoroutine;

        public bool IsFull => _currentScore >= _scoreToFill;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _originalScale = transform.localScale;
        }

        public void SetScoreToFill(float score)
        {
            _scoreToFill = score;
            _currentScore = 0;
            _image.fillAmount = 0;
        }

        public void SetFill(float normalized)
        {
            Debug.Log("SetFill " + normalized);
            _currentScore = Mathf.Clamp(normalized * _scoreToFill, 0, _scoreToFill);
            _image.fillAmount = _currentScore / _scoreToFill;
        }

        public void PlayUnlockEffects()
        {
            if (_particle != null)
                _particle.Play();

            if (_popCoroutine != null)
                StopCoroutine(_popCoroutine);

            _popCoroutine = StartCoroutine(PopAnimation());
        }

        private IEnumerator PopAnimation()
        {
            float t = 0;
            Vector3 targetScale = _originalScale * _popScale;
            float startFill = _image.fillAmount;

            // Увеличение
            while (t < _popTime)
            {
                t += Time.deltaTime;
                float p = t / _popTime;
                transform.localScale = Vector3.Lerp(_originalScale, targetScale, p);
                _image.fillAmount = Mathf.Lerp(startFill, _image.fillAmount, p);
                yield return null;
            }

            // Возврат
            t = 0;
            while (t < _popTime)
            {
                t += Time.deltaTime;
                float p = t / _popTime;
                transform.localScale = Vector3.Lerp(targetScale, _originalScale, p);
                yield return null;
            }

            transform.localScale = _originalScale;
            _popCoroutine = null;
        }
    }
}
