using System.Collections.Generic;
using UnityEngine;

namespace DreamVille.UI
{
    public class StarsUI : MonoBehaviour
    {
        [SerializeField] private List<ScoreStar> _scoreStars;

        public void SetStarProgress(int index, float normalized)
        {
            if (index < 0 || index >= _scoreStars.Count) return;
            _scoreStars[index].SetFill(normalized);
        }

        public void UnlockStar(int index)
        {
            if (index < 0 || index >= _scoreStars.Count) return;
            _scoreStars[index].PlayUnlockEffects();
        }
    }
}
