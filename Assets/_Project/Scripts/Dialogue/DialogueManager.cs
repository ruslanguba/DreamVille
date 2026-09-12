using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Serialization;
using Zenject;

namespace DreamVille.Dialogue
{
    /// <summary>
    /// Dialogue presenter: reflects the state of <see cref="DialogueService"/> on screen.
    /// The sentence queue lives in the service, input in <see cref="DialogueInputHandler"/>.
    /// </summary>
    public class DialogueManager : MonoBehaviour
    {
        [FormerlySerializedAs("dialogueUIPrefab")]
        [SerializeField] private DialogueUI _dialogueUIPrefab;

        private DiContainer _container;
        private DialogueService _dialogue;
        private DialogueUI _ui;

        /// <remarks>
        /// The container is injected only to instantiate the UI prefab through Zenject so that
        /// <see cref="DialogueUI"/> receives its own dependencies; it is not used as a locator.
        /// </remarks>
        [Inject]
        public void Construct(DiContainer container, DialogueService dialogue)
        {
            _container = container;
            _dialogue = dialogue;
        }

        private void Start()
        {
            EnsureEventSystem();

            if (_dialogueUIPrefab == null)
            {
                Debug.LogError($"[{nameof(DialogueManager)}] '{name}' has no dialogue UI prefab assigned.", this);
                return;
            }

            _ui = _container.InstantiatePrefabForComponent<DialogueUI>(_dialogueUIPrefab);

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

        /// <summary>
        /// Fallback for scenes that do not ship their own <see cref="EventSystem"/>.
        /// Prefer wiring one in the scene explicitly.
        /// </summary>
        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var go = new GameObject(nameof(EventSystem));
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }
    }
}
