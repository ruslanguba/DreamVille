using System;
using UnityEngine;

using DreamVille.Dialogue;

namespace DreamVille.Progression
{
    /// <summary>
    /// Финальная зона уровня: ведёт себя как один самодостаточный объект —
    /// озвучивает свой диалог и умеет открыть проход. Про прогресс, NPC и сценарий не знает,
    /// связи с остальным уровнем создаёт <see cref="LevelDirector"/>.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class FinalZone : MonoBehaviour
    {
        private const string OpenTrigger = "start";

        /// <summary>Финальный диалог закончился — можно открывать проход.</summary>
        public event Action DialogueFinished;

        [SerializeField] private DialogueTrigger _dialogue;
        [SerializeField] private Animator _animator;
        [SerializeField] private AudioSource _audio;
        [SerializeField] private AudioClip[] _mumbleClips;

        private void Awake()
        {
            // Связи внутри одного префаба: includeInactive и проверка на null обязательны.
            if (_dialogue == null) _dialogue = GetComponentInChildren<DialogueTrigger>(true);
            if (_animator == null) _animator = GetComponent<Animator>();
            if (_audio == null) _audio = GetComponent<AudioSource>();

            // До победы зона не реагирует на игрока. Объект при этом остаётся активным:
            // выключенный триггер терял ссылки и всё равно ловил вход.
            SetArmed(false);
        }

        private void OnEnable()
        {
            if (_dialogue == null)
                return;

            _dialogue.OnDialogueStarted += PlayMumble;
            _dialogue.OnDialogueEnded += OnDialogueEnded;
        }

        private void OnDisable()
        {
            if (_dialogue == null)
                return;

            _dialogue.OnDialogueStarted -= PlayMumble;
            _dialogue.OnDialogueEnded -= OnDialogueEnded;
        }

        /// <summary>Взвести зону: с этого момента вход игрока запускает финальный диалог.</summary>
        public void Arm() => SetArmed(true);

        /// <summary>Проиграть анимацию открытия прохода.</summary>
        public void OpenPortal()
        {
            if (_audio != null)
                _audio.Stop();

            if (_animator != null)
                _animator.SetTrigger(OpenTrigger);
        }

        private void SetArmed(bool isArmed)
        {
            if (_dialogue != null)
                _dialogue.IsArmed = isArmed;
        }

        private void PlayMumble()
        {
            if (_audio == null || _mumbleClips == null || _mumbleClips.Length == 0)
                return;

            _audio.PlayOneShot(_mumbleClips[UnityEngine.Random.Range(0, _mumbleClips.Length)]);
        }

        private void OnDialogueEnded() => DialogueFinished?.Invoke();
    }
}
