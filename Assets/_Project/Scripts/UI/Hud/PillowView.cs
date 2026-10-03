using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DreamVille.UI.Hud
{
    /// <summary>HUD row of pillow icons: one per available pillow, visible while that pillow is in hand.</summary>
    public sealed class PillowView : MonoBehaviour, IPillowView
    {
        [SerializeField] private Image _iconPrefab;
        [SerializeField] private RectTransform _firstPosition;
        [SerializeField] private RectTransform _pillowsPanel;
        [SerializeField] private float _posOffset = 160f;
        [SerializeField] private float _alphaHidden = 0f;

        private readonly List<Image> _pillowIcons = new List<Image>();

        /// <summary>
        /// Shows <paramref name="total"/> icons; the first <paramref name="inHand"/> are visible, the rest hidden.
        /// </summary>
        public void Show(int total, int inHand)
        {
            total = Mathf.Max(0, total);
            if (_pillowIcons.Count != total)
                SpawnIcons(total);

            // The prefab's own alpha is the visible look.
            float alphaVisible = _iconPrefab.color.a;
            for (int i = 0; i < _pillowIcons.Count; i++)
            {
                if (_pillowIcons[i] != null)
                    _pillowIcons[i].SetAlpha(i < inHand ? alphaVisible : _alphaHidden);
            }
        }

        private void SpawnIcons(int count)
        {
            ClearIcons();

            Vector3 startPos = _firstPosition.localPosition;

            for (int i = 0; i < count; i++)
            {
                Image icon = Instantiate(_iconPrefab, _pillowsPanel);
                icon.transform.localPosition = startPos + new Vector3(i * _posOffset, 0f, 0f);
                _pillowIcons.Add(icon);
            }
        }

        private void ClearIcons()
        {
            foreach (Image icon in _pillowIcons)
            {
                if (icon != null)
                    Destroy(icon.gameObject);
            }

            _pillowIcons.Clear();
        }
    }
}
