using UnityEngine;
using Zenject;

using DreamVille.Scoring;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>
    /// Paints an object stroke by stroke: each stroke gives score and advances the colour-change shader until the
    /// object is fully painted; further strokes do nothing.
    /// </summary>
    public sealed class PaintEffect : MonoBehaviour, IInteractionEffect
    {
        // Overridden per instance in Lobby_02; renames follow ADR 0010.
        [SerializeField] private Renderer[] _renderer;
        [SerializeField] private Material _colorChangeMaterial;

        [SerializeField, Min(1)] private int _strokeCount = 5;
        [SerializeField] private float _scorePerStroke = 10f;

        private IScoreService _score;
        private PaintProgress _progress;

        [Inject]
        private void Construct(IScoreService score)
        {
            _score = score;
        }

        private void Awake()
        {
            _progress = new PaintProgress(_strokeCount);

            if (_colorChangeMaterial == null)
            {
                Debug.LogError($"[{nameof(PaintEffect)}] Colour-change material is not assigned.", this);
                return;
            }

            // Every renderer gets its own copy so the paint ramp of this object does not leak into others.
            foreach (Renderer target in _renderer)
                target.material = new Material(_colorChangeMaterial);
        }

        public void Apply(in InteractionContext context)
        {
            if (!_progress.TryAddStroke())
                return;

            _score.AddScore(_scorePerStroke);
            foreach (Renderer target in _renderer)
                target.material.SetFloat(ShaderProps.Transient, _progress.Transient);
        }
    }
}
