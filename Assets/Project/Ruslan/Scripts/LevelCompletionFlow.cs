using UnityEngine;

public class LevelCompletionFlow : MonoBehaviour
{
    [SerializeField] private NPCReactionController _npc;
    [SerializeField] private DialogueTrigger _trigger;
    [SerializeField] private PortalActivator _portalActivator;

    void Start()
    {
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
        _portalActivator.ActivatePortal();
    }

    private void OnDisable()
    {
        _trigger.OnDialogueStarted -= StartNPCTalking;
        _trigger.OnDialogueEnded -= FinalAction;
    }

    public virtual void LevelCompleat()
    {
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


