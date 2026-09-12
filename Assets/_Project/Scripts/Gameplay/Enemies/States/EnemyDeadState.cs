using DreamVille.Configs.Enemies;

namespace DreamVille.Gameplay.Enemies.States
{
    /// <summary>Terminal state: releases the body to physics, plays the death animation and despawns.</summary>
    public sealed class EnemyDeadState : EnemyStateBase
    {
        public EnemyDeadState(IEnemyContext context, EnemyStateMachine machine) : base(context, machine) { }

        public override EnemyStateId Id => EnemyStateId.Dead;

        public override void Enter()
        {
            _context.SetAutoFacing(false);
            _context.ReleaseBody();
            _context.Animator.TriggerDeath();
            _context.Despawn(_context.Config.DespawnDelay);
        }
    }
}
