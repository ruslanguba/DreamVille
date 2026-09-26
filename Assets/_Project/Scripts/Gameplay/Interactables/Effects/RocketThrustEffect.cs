using UnityEngine;

namespace DreamVille.Gameplay.Interactables.Effects
{
    /// <summary>
    /// Launches a rocket: a push forward, engine particles, then continuous thrust and a spin around its axis until it
    /// is destroyed. Every further hit pushes it again; the rocket is still destroyed on the timer the first hit set.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public sealed class RocketThrustEffect : MonoBehaviour, IInteractionEffect
    {
        private const float SpinDegreesPerSecond = 600f;

        [SerializeField] private float _thrustPower = 200f;
        [SerializeField] private float _continuousThrust = 500f;
        [SerializeField] private ParticleSystem _particle;
        [SerializeField, Min(0f)] private float _lifetimeAfterLaunch = 20f;

        private Rigidbody _rigidbody;
        private bool _launched;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();

            if (_particle == null)
                Debug.LogError($"[{nameof(RocketThrustEffect)}] Engine particles are not assigned.", this);
        }

        private void Update()
        {
            if (_launched)
                transform.Rotate(0f, 0f, SpinDegreesPerSecond * Time.deltaTime);
        }

        private void FixedUpdate()
        {
            if (_launched)
                _rigidbody.AddForce(transform.forward * (_continuousThrust * Time.fixedDeltaTime), ForceMode.Acceleration);
        }

        public void Apply(in InteractionContext context)
        {
            _rigidbody.AddForce(transform.forward * _thrustPower, ForceMode.Force);
            if (_particle != null)   // missing particles are reported once in Awake
                _particle.Play();
            _launched = true;
            Destroy(gameObject, _lifetimeAfterLaunch);
        }
    }
}
