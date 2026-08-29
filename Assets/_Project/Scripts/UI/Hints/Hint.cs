using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DreamVille.UI.Hints
{
    public class Hint : MonoBehaviour
    {
        public string HintText => _hintText;
        [TextArea(3,9)]
        [SerializeField] private string _hintText;
    }
}
