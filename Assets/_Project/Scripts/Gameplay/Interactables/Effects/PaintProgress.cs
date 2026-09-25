using System;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>Paint strokes left until an object is fully painted; drives the shader's transient value.</summary>
    public sealed class PaintProgress
    {
        /// <summary>Shader value at full progress (the material ramps 0..2).</summary>
        public const float TransientAtComplete = 2f;

        public PaintProgress(int strokeCount)
        {
            if (strokeCount < 1)
                throw new ArgumentOutOfRangeException(nameof(strokeCount), strokeCount, "A paintable needs at least one stroke.");

            StrokeCount = strokeCount;
        }

        /// <summary>Strokes needed to paint the object completely.</summary>
        public int StrokeCount { get; }

        /// <summary>Strokes applied so far, never above <see cref="StrokeCount"/>.</summary>
        public int Strokes { get; private set; }

        public bool IsComplete => Strokes >= StrokeCount;

        /// <summary>Strokes / StrokeCount * <see cref="TransientAtComplete"/>.</summary>
        public float Transient => (float)Strokes / StrokeCount * TransientAtComplete;

        /// <summary>Adds one stroke; false once complete (no score, no repaint).</summary>
        public bool TryAddStroke()
        {
            if (IsComplete)
                return false;

            Strokes++;
            return true;
        }
    }
}
