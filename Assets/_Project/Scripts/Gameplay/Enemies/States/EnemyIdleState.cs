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

            // Idle has no tick to finish a gradual turn, so it faces the player at once, and keeps that facing
            // while the body settles instead of turning after its own drift.
            _context.SetAutoFacing(false);
            if (_context.HasPlayer)
                _context.SnapFacing(_context.PlayerPosition);
        }

        public override void Exit()
        {
            _context.SetAutoFacing(true);
        }
    }
}
