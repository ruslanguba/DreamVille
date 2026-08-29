using UnityEngine;

namespace DreamVille.Dialogue
{
    [System.Serializable]
    public class Dialogue
    {
        public bool IsPluh;
        [TextArea(3,9)]
        public string Sentence;
    }
}
