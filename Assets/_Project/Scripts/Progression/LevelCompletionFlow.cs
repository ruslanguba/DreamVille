using UnityEngine;

using DreamVille.Dialogue;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Interactables;
using DreamVille.Gameplay.NPC;

namespace DreamVille.Progression
{
    public class LevelCompletionFlow : MonoBehaviour
    {
        [SerializeField] private NPCReactionController _npc;
        [SerializeField] private DialogueTrigger _trigger;
        [SerializeField] private PortalActivator _portalActivator;

        void Start()
        {
            if (_trigger == null)
            {
                Debug.LogWarning($"{nameof(LevelCompletionFlow)} на '{name}': не назначен {nameof(_trigger)}.", this);
                return;
            }

            _trigger.OnDialogueStarted += StartNPCTalking;
            _trigger.OnDialogueEnded += FinalAction;
            _trigger.DisableTrigger();
        }

        public void StartNPCTalking()
        {
            _npc.SetApplause();
        }

        private void FinalAction()
        {
            _npc.SetIdle();

            if (_portalActivator == null)
            {
                Debug.LogWarning($"{nameof(LevelCompletionFlow)} на '{name}': не назначен {nameof(_portalActivator)}, портал не откроется.", this);
                return;
            }

            _portalActivator.ActivatePortal();
        }

        private void OnDisable()
        {
            if (_trigger == null)
                return;

            _trigger.OnDialogueStarted -= StartNPCTalking;
            _trigger.OnDialogueEnded -= FinalAction;
        }

        public virtual void LevelCompleat()
        {
            if (_trigger == null)
                return;

            _trigger.EnableTrigger();
            _npc.SetGreeting();
        }
    }


    //{
    //    [Header("Actors")]
    //    [SerializeField] private NPCReactionController _npc;
    //    [SerializeField] private DialogueTrigger _dialogue;
    //    [SerializeField] private PortalActivator _portal;

    //    [Header("Save")]
    //    [SerializeField] private string _levelId;

    //    public void Initialize(EnemyRegistry enemyRegistry)
    //    {
    //        enemyRegistry.OnAllEnemiesKilled += StartFlow;
    //    }

    //    private void StartFlow()
    //    {
    //        _npc.PlayGreeting();          // анимация + звук
    //        _dialogue.Enable();           // включаем триггер
    //        _dialogue.OnDialogueEnded += OnDialogueFinished;
    //    }

    //    private void OnDialogueFinished()
    //    {
    //        _dialogue.OnDialogueEnded -= OnDialogueFinished;

    //        _npc.SetApplause();    // финальная анимация
    //        _portal.Enable();             // портал
    //    }


    //}
}
