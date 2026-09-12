using UnityEngine;

using DreamVille.Configs.Enemies;

namespace DreamVille.Gameplay.Enemies.States
{
    /// <summary>Runs the enemy straight at the player until it is close enough to attack or too far to chase.</summary>
    public sealed class EnemyChaseState : EnemyStateBase
    {
        public EnemyChaseState(IEnemyContext context, EnemyStateMachine machine) : base(context, machine) { }

        public override EnemyStateId Id => EnemyStateId.Chase;

        public override void Tick(float deltaTime)
        {
            if (!_context.HasPlayer)
                return;

            float distance = Vector3.Distance(_context.Position, _context.PlayerPosition);

            if (distance < _context.Tuning.AttackRange)
            {
                _machine.ChangeState(EnemyStateId.Attack);
                return;
            }

            if (distance > _context.Tuning.ChaseRange)
            {
                _machine.ChangeState(EnemyStateId.Patrol);
                return;
            }
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            if (!_context.HasPlayer)
                return;

            _context.Steering.Accelerate(_context.PlayerPosition - _context.Position, _context.Tuning.MoveSpeed, _context.Config.Acceleration);
        }
    }
}
