using System.Collections.Generic;
using UnityEngine;

namespace DreamVille.UI.Hud
{
    /// <summary>Row of <see cref="ScoreStar"/> meters, addressed by zero-based index.</summary>
    public sealed class StarsView : MonoBehaviour, IStarsView
    {
        [SerializeField] private List<ScoreStar> _scoreStars;

        public void SetProgress(int starIndex, float normalized)
        {
            if (!IsValidIndex(starIndex))
                return;

            _scoreStars[starIndex].SetFill(normalized);
        }

        public void PlayUnlock(int starIndex)
        {
            if (!IsValidIndex(starIndex))
                return;

            _scoreStars[starIndex].PlayUnlock();
        }

        private bool IsValidIndex(int starIndex)
            => starIndex >= 0 && starIndex < _scoreStars.Count;
    }
}
