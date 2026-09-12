namespace DreamVille.Configs.Enemies
{
    /// <summary>Identifies a state in the enemy state machine.</summary>
    public enum EnemyStateId
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        FleeToHideout,
        Hidden,
        Dead
    }
}
