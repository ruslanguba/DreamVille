using System.Collections;
using UnityEngine.UI;
using UnityEngine;

using DreamVille.Gameplay.Weapons.Bullets;

namespace DreamVille.UI
{
    public class RicochetUI : MonoBehaviour
    {
        [SerializeField] private Image[] _images;
        private Camera _camera;

        private void Awake()
        {
            _camera = Camera.main;
        }

        public void OnBulletHit(BulletHitInfo info)
        {
            int index = info.RicochetCount - 2;
            if (index < 0 || index >= _images.Length) return;

            var img = _images[index];
            img.gameObject.SetActive(true);
            img.rectTransform.position = _camera.WorldToScreenPoint(info.HitPoint);
            StartCoroutine(Hide(img));
        }

        private IEnumerator Hide(Image img)
        {
            yield return new WaitForSeconds(0.5f);
            img.gameObject.SetActive(false);
        }
    }
}
