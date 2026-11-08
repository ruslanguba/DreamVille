using UnityEngine;

namespace DreamVille.Audio
{
    /// <summary>An actor's sounds from one AudioSource: death, attack and jump one-shots, and footsteps while it moves.</summary>
    [RequireComponent(typeof(AudioSource))]
    public sealed class ActorAudio : MonoBehaviour, IActorAudio
    {
        [SerializeField] private AudioClip _deathClip;
        [SerializeField] private AudioClip _attackClip;
        [SerializeField] private AudioClip _jumpClip;

        [SerializeField, Min(0f), Tooltip("Maximum random pitch offset of a jump, either way.")]
        private float _jumpPitchJitter = 0.1f;

        [Header("Footsteps")]
        [SerializeField] private AudioClip[] _footstepClips;

        [SerializeField, Min(0f), Tooltip("Seconds of game time between two steps while moving.")]
        private float _stepInterval = 0.5f;

        [SerializeField, Min(0f), Tooltip("Maximum random pitch offset of a step, either way.")]
        private float _footstepPitchJitter = 0.1f;

        private AudioSource _audioSource;
        private int _lastFootstepIndex = -1;
        private float _stepTimer;

        // Also resolved on first use, so the component works before Awake (EditMode tests).
        private AudioSource Source => _audioSource != null ? _audioSource : _audioSource = GetComponent<AudioSource>();

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void PlayDeath() => PlayAtNeutralPitch(_deathClip);

        public void PlayAttack() => PlayAtNeutralPitch(_attackClip);

        public void PlayJump()
        {
            if (_jumpClip == null)
                return;

            Source.pitch = 1f + Random.Range(-_jumpPitchJitter, _jumpPitchJitter);
            Source.PlayOneShot(_jumpClip);
        }

        public void TickFootsteps(bool isMoving)
        {
            if (!isMoving)
            {
                _stepTimer = 0;
                return;
            }

            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0f)
            {
                PlayStep();
                _stepTimer = _stepInterval;
            }
        }

        // The source is shared with the jittered jump and footsteps; a plain one-shot must not inherit their pitch.
        private void PlayAtNeutralPitch(AudioClip clip)
        {
            if (clip == null)
                return;

            Source.pitch = 1f;
            Source.PlayOneShot(clip);
        }

        private void PlayStep()
        {
            if (_footstepClips == null || _footstepClips.Length == 0)
                return;

            // Random index, but never the same clip twice in a row.
            int index;
            do
            {
                index = Random.Range(0, _footstepClips.Length);
            }
            while (index == _lastFootstepIndex && _footstepClips.Length > 1);

            _lastFootstepIndex = index;

            Source.pitch = 1f + Random.Range(-_footstepPitchJitter, _footstepPitchJitter);
            Source.PlayOneShot(_footstepClips[index]);
        }
    }
}
