using UnityEngine;

namespace DreamVille.App
{
    /// <summary>
    /// Default <see cref="IApplicationQuitter"/>: <c>Application.Quit</c> in a player,
    /// <c>EditorApplication.isPlaying = false</c> in the editor, where <c>Application.Quit</c> is ignored.
    /// </summary>
    public sealed class UnityApplicationQuitter : IApplicationQuitter
    {
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
