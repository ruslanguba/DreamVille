using UnityEngine;

using DreamVille.Configs.Enemies;

namespace DreamVille.Gameplay.Enemies.States
{
    /// <summary>Runs the enemy to its flee target and hands it to the hideout, or idles, on arrival.</summary>
    public sealed class EnemyFleeToHideoutState : EnemyStateBase
    {
        public EnemyFleeToHideoutState(IEnemyContext context, EnemyStateMachine machine) : base(context, machine) { }

        public override EnemyStateId Id => EnemyStateId.FleeToHideout;

        public override void Enter()
        {
            _context.Animator.SetMoving(true);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            if (!_context.HasFleeTarget)
                return;

            _context.Steering.Accelerate(_context.FleeTargetPosition - _context.Position, _context.Config.MoveSpeed, _context.Config.Acceleration);

            float distance = Vector3.Distance(_context.Position, _context.FleeTargetPosition);
            if (distance >= _context.Config.HideoutArriveRadius)
                return;

            _context.SetVulnerable(true);
            _context.Animator.SetMoving(false);
            _machine.ChangeState(_context.FleeTargetIsHideout ? EnemyStateId.Hidden : EnemyStateId.Idle);
        }
    }
}
