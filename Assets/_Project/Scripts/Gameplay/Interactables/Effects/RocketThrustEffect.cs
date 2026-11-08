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

        [SerializeField, Min(0f), Tooltip("Speed change in m/s the first hit gives along the rocket's forward, whatever its mass.")]
        private float _launchSpeedChange = 4f;

        [SerializeField, Min(0f), Tooltip("Acceleration in m/s² along the rocket's forward while it flies; independent of the physics step.")]
        private float _thrustAcceleration = 10f;

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
                _rigidbody.AddForce(transform.forward * _thrustAcceleration, ForceMode.Acceleration);
        }

        public void Apply(in InteractionContext context)
        {
            _rigidbody.AddForce(transform.forward * _launchSpeedChange, ForceMode.VelocityChange);
            if (_particle != null)   // missing particles are reported once in Awake
                _particle.Play();
            _launched = true;
            Destroy(gameObject, _lifetimeAfterLaunch);
        }
    }
}
