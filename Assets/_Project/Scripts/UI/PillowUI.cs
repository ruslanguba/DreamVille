using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

namespace DreamVille.UI
{
    public class PillowUI : MonoBehaviour
    {
        [SerializeField] private Image _iconPrefab;
        [SerializeField] private RectTransform _firstPosition;
        [SerializeField] private RectTransform _pillowsPanel;
        [SerializeField] private float _posOffset = 160f;
        [SerializeField] private float _alphaVisible;
        [SerializeField] private float _alphaHidden = 0f;

        private int _usedPillows;
        private List<Image> _pillowIcons = new List<Image>();

        /// <summary>Creates one icon per pillow slot.</summary>
        public void SpawnIcons(int count)
        {
            Vector3 startPos = _firstPosition.localPosition;

            for (int i = 0; i < count; i++)
            {
                Image icon = Instantiate(_iconPrefab, _pillowsPanel);
                icon.transform.localPosition = startPos + new Vector3(i * _posOffset, 0f, 0f);
                _alphaVisible = icon.color.a;

                _pillowIcons.Add(icon);
            }
        }

        /// <summary>Shows the icon for a pillow that was recalled.</summary>
        public void ShowPillow()
        {
            if (_usedPillows <= 0)
                return;

            int index = _pillowIcons.Count - _usedPillows;
            if (index < 0 || index >= _pillowIcons.Count)
                return;

            SetAlpha(_pillowIcons[index], _alphaVisible);
            _usedPillows--;
        }

        /// <summary>Hides the icon for a pillow that was thrown.</summary>
        public void HidePillow()
        {
            if (_usedPillows >= _pillowIcons.Count)
                return;

            _usedPillows++;
            for (int i = _pillowIcons.Count - 1; i >= 0; i--)
            {
                if (_pillowIcons[i].color.a > 0.9f)
                {
                    SetAlpha(_pillowIcons[i], _alphaHidden);
                    break;
                }
            }
        }

        private void SetAlpha(Image image, float alpha)
        {
            if (image == null) return;
            Color c = image.color;
            c.a = Mathf.Clamp01(alpha);
            image.color = c;
        }

        private void ClearIcons()
        {
            foreach (var icon in _pillowIcons)
                if (icon != null) Destroy(icon.gameObject);
            _pillowIcons.Clear();
        }
    }
}
