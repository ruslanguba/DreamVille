using System;
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
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private Material _colorChangeMaterial;

        [SerializeField, Min(1)] private int _strokeCount = 5;
        [SerializeField] private float _scorePerStroke = 10f;

        private IScoreService _score;
        private PaintProgress _progress;

        // The per-renderer copies of the colour-change material; created here, so destroyed here.
        private Material[] _materials = Array.Empty<Material>();

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
            _materials = new Material[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _materials[i] = new Material(_colorChangeMaterial);
                _renderers[i].material = _materials[i];
            }
        }

        private void OnDestroy()
        {
            foreach (Material material in _materials)
                Destroy(material);

            _materials = Array.Empty<Material>();
        }

        public void Apply(in InteractionContext context)
        {
            if (!_progress.TryAddStroke())
                return;

            _score.AddScore(_scorePerStroke);
            foreach (Material material in _materials)
                material.SetFloat(ShaderProps.Transient, _progress.Transient);
        }
    }
}
