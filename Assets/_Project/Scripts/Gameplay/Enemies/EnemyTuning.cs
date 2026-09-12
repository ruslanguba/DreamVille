namespace DreamVille.Gameplay.Enemies
{
    /// <summary>Per-instance tuning snapshot taken once in Awake.</summary>
    public readonly struct EnemyTuning
    {
        public EnemyTuning(float moveSpeed, float chaseRange, float attackRange, float attackCooldown)
        {
            MoveSpeed = moveSpeed;
            ChaseRange = chaseRange;
            AttackRange = attackRange;
            AttackCooldown = attackCooldown;
        }

        public float MoveSpeed { get; }
        public float ChaseRange { get; }
        public float AttackRange { get; }
        public float AttackCooldown { get; }
    }
}
