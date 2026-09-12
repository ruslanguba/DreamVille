using DreamVille.Configs.Enemies;

namespace DreamVille.Gameplay.Enemies.States
{
    /// <summary>Stationary state used before an enemy is released from a hideout or spawns active.</summary>
    public sealed class EnemyIdleState : EnemyStateBase
    {
        public EnemyIdleState(IEnemyContext context, EnemyStateMachine machine) : base(context, machine) { }

        public override EnemyStateId Id => EnemyStateId.Idle;

        public override void Enter()
        {
            _context.Steering.Stop();
            _context.Animator.SetMoving(false);

            if (_context.HasPlayer)
                _context.FaceTowards(_context.PlayerPosition);
        }
    }
}
