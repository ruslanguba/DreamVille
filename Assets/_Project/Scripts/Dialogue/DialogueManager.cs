using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace DreamVille.Dialogue
{
    /// <summary>
    /// Презентер диалога: показывает состояние <see cref="DialogueService"/> на экране.
    /// Логика очереди реплик живёт в сервисе, ввод — в DialogueInputHandler.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogueUIPrefab;

        private DialogueService _dialogue;
        private DialogueUI _ui;

        [Inject]
        public void Construct(DialogueService dialogue)
        {
            _dialogue = dialogue;
        }

        private void Start()
        {
            EnsureEventSystem();

            _ui = Instantiate(dialogueUIPrefab);

            _dialogue.ActiveChanged += OnActiveChanged;
            _dialogue.SentenceShown += OnSentenceShown;
        }

        private void OnDestroy()
        {
            if (_dialogue == null)
                return;

            _dialogue.ActiveChanged -= OnActiveChanged;
            _dialogue.SentenceShown -= OnSentenceShown;
        }

        private void OnActiveChanged(bool isActive)
        {
            if (isActive)
                _ui.ShowPanel();
            else
                _ui.HidePanel();
        }

        private void OnSentenceShown(Dialogue sentence)
        {
            _ui.ShowNextSentence(sentence.IsPluh, sentence.Sentence);
        }

        // TODO: EventSystem должен лежать в сцене, а не создаваться в рантайме.
        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var go = new GameObject(nameof(EventSystem));
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }
    }
}
