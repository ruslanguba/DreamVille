using UnityEngine;

using DreamVille.Configs.Enemies;

namespace DreamVille.Gameplay.Enemies.States
{
    /// <summary>Stands its ground, faces the player and throws on a cooldown that survives re-entry.</summary>
    public sealed class EnemyAttackState : EnemyStateBase
    {
        private float _cooldown;

        public EnemyAttackState(IEnemyContext context, EnemyStateMachine machine) : base(context, machine) { }

        public override EnemyStateId Id => EnemyStateId.Attack;

        public override void Enter()
        {
            _context.SetAutoFacing(false);
            _context.Steering.Stop();

            if (_context.HasPlayer)
                _context.SnapFacing(_context.PlayerPosition);
        }

        public override void Tick(float deltaTime)
        {
            if (!_context.HasPlayer)
                return;

            _context.FaceTowards(_context.PlayerPosition);

            float distance = Vector3.Distance(_context.Position, _context.PlayerPosition);
            if (distance > _context.Config.AttackRange)
            {
                _machine.ChangeState(EnemyStateId.Chase);
                return;
            }

            _cooldown -= deltaTime;
            if (_cooldown <= 0f)
            {
                _context.Attack.Throw((_context.PlayerPosition - _context.Attack.Origin).normalized);
                _context.Audio.PlayAttack();
                _cooldown = _context.Config.AttackCooldown;
            }
        }

        public override void Exit()
        {
            _context.SetAutoFacing(true);
        }
    }
}
