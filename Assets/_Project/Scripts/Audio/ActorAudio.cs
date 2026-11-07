using UnityEngine;

namespace DreamVille.Audio
{
    /// <summary>An actor's sounds from one AudioSource: death, attack and jump one-shots, footsteps while it moves, and random ambient clips on a timer.</summary>
    [RequireComponent(typeof(AudioSource))]
    public class ActorAudio : MonoBehaviour, IActorAudio
    {
        private AudioSource _audioSource;

        // Also resolved on first use, so the component works before Awake (EditMode tests).
        private AudioSource Source => _audioSource != null ? _audioSource : _audioSource = GetComponent<AudioSource>();
        [SerializeField] private AudioClip _deathClip;
        [SerializeField] private AudioClip _attackClip;
        [SerializeField] private AudioClip _jumpClip;
        [SerializeField] private float _jumpPitchJitter = 0.1f;

        [Header("Footsteps")]
        [SerializeField] private AudioClip[] _footstepClips;
        [SerializeField] private float _stepInterval = 0.5f;
        [SerializeField] private float _footstepPitchJitter = 0.1f;

        private int _lastFootstepIndex = -1;

        [Header("Random Clips")]
        [SerializeField] private AudioClip[] _randomClips;
        [SerializeField] private Vector2 _randomClipInterval = new Vector2(3f, 7f);
        [SerializeField] private float _randomClipPitchJitter = 0.15f;

        private float _nextRandomTime;
        private float _stepTimer;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }
        private void Start()
        {
            ScheduleNextRandom();
        }
        private void Update()
        {
            // Play a random ambient clip whenever the timer runs out.
            if (Time.time >= _nextRandomTime)
            {
                PlayRandomClip();
                ScheduleNextRandom();
            }
        }
        private void PlayRandomClip()
        {
            if (_randomClips.Length == 0) return;

            Source.pitch = 1f + Random.Range(-_randomClipPitchJitter, _randomClipPitchJitter);
            var clip = _randomClips[Random.Range(0, _randomClips.Length)];
            Source.PlayOneShot(clip);
        }

        private void ScheduleNextRandom()
        {
            _nextRandomTime = Time.time + Random.Range(_randomClipInterval.x, _randomClipInterval.y);
        }
        public void PlayDeath() => PlayAtNeutralPitch(_deathClip);

        public void PlayAttack() => PlayAtNeutralPitch(_attackClip);

        // The source is shared with the jittered jump and footsteps; a plain one-shot must not inherit their pitch.
        private void PlayAtNeutralPitch(AudioClip clip)
        {
            if (clip == null)
                return;

            Source.pitch = 1f;
            Source.PlayOneShot(clip);
        }

        public void PlayJump()
        {
            if (_jumpClip != null)
            {
                Source.pitch = 1f + Random.Range(-_jumpPitchJitter, _jumpPitchJitter);
                Source.PlayOneShot(_jumpClip);
            }
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
