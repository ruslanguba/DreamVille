using UnityEngine.UI;
using UnityEngine;

namespace DreamVille.UI
{
    /// <summary>
    /// Draws a ParticleSystem inside a UGUI canvas by baking its mesh through a camera.
    /// The mesh is rebuilt only while the system is alive, plus once more when it dies so no stale particles stay drawn.
    /// </summary>
    public sealed class UIParticleSystem : MaskableGraphic
    {
        [SerializeField] private ParticleSystemRenderer _renderer;
        [SerializeField] private Texture _texture;
        [SerializeField] private Camera _bakeCamera;

        private ParticleSystem _particleSystem;
        private bool _wasAlive;

        public override Texture mainTexture => _texture != null ? _texture : base.mainTexture;

        protected override void Awake()
        {
            base.Awake();

            if (_renderer != null)
                _particleSystem = _renderer.GetComponent<ParticleSystem>();
        }

        private void Update()
        {
            bool isAlive = _particleSystem != null && _particleSystem.IsAlive(true);

            if (isAlive || _wasAlive)
                SetVerticesDirty();

            _wasAlive = isAlive;
        }

        protected override void OnPopulateMesh(Mesh mesh)
        {
            mesh.Clear();

            if (_renderer != null && _bakeCamera != null)
            {
                _renderer.BakeMesh(mesh, _bakeCamera);
            }
        }
    }
}
