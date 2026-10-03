using UnityEngine;
using UnityEngine.UI;

namespace DreamVille.UI
{
    /// <summary>Alpha changes for UGUI graphics that keep the authored colour.</summary>
    public static class GraphicExtensions
    {
        /// <summary>Sets <paramref name="graphic"/>'s alpha, clamped to [0, 1]; red, green and blue are unchanged.</summary>
        /// <remarks>Goes through <see cref="Graphic.color"/>, so TextMeshPro applies it to its font colour.</remarks>
        public static void SetAlpha(this Graphic graphic, float alpha)
        {
            Color color = graphic.color;
            color.a = Mathf.Clamp01(alpha);
            graphic.color = color;
        }
    }
}
