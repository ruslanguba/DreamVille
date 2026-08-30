using UnityEngine;
using Zenject;

using DreamVille.Gameplay.NPC;

namespace DreamVille.Legacy
{
    /// <summary>
    /// Реакция NPC на финал уровня. Вешается на нужного NPC —
    /// тем самым сцена и решает, кто именно реагирует.
    /// </summary>
    [RequireComponent(typeof(NPCReactionController))]
    public class NpcOutroReaction : MonoBehaviour
    {
        private NPCReactionController _reactions;
        private LevelOutroService _outro;

        [Inject]
        public void Construct(LevelOutroService outro)
        {
            _outro = outro;
        }

        private void Awake()
        {
            _reactions = GetComponent<NPCReactionController>();
        }

        private void Start()
        {
            if (_outro == null)
                return;

            _outro.Started += OnOutroStarted;
            _outro.DialogueStarted += OnDialogueStarted;
            _outro.Finished += OnFinished;
        }

        private void OnDestroy()
        {
            if (_outro == null)
                return;

            _outro.Started -= OnOutroStarted;
            _outro.DialogueStarted -= OnDialogueStarted;
            _outro.Finished -= OnFinished;
        }

        private void OnOutroStarted() => _reactions.SetGreeting();

        private void OnDialogueStarted() => _reactions.SetApplause();

        private void OnFinished() => _reactions.SetIdle();
    }
}
