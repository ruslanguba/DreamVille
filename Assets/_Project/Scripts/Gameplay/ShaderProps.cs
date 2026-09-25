using UnityEngine;

namespace DreamVille.Gameplay
{
    /// <summary>Cached shader property IDs. Shader property names live here only.</summary>
    public static class ShaderProps
    {
        /// <summary>Paint ramp of the colour-change material (0 = unpainted, 2 = fully painted).</summary>
        public static readonly int Transient = Shader.PropertyToID("_Transient");
    }
}
