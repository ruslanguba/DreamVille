using UnityEngine;

using DreamVille.Configs.Enemies;

namespace DreamVille.Gameplay.Enemies.States
{
    /// <summary>
    /// Walks the enemy's patrol route until the player enters chase or attack range. Route progress survives
    /// leaving and re-entering the state: after a chase the enemy heads for the point it was walking to, and a
    /// route walked once (or a single post) is never walked again.
    /// </summary>
    public sealed class EnemyPatrolState : EnemyStateBase
    {
        private int _index;
        private bool _hasStoppedAtEnd;

        public EnemyPatrolState(IEnemyContext context, EnemyStateMachine machine) : base(context, machine) { }

        public override EnemyStateId Id => EnemyStateId.Patrol;

        public override void Tick(float deltaTime)
        {
            if (!_context.HasPlayer)
                return;

            float distance = Vector3.Distance(_context.Position, _context.PlayerPosition);

            if (distance < _context.Config.ChaseRange)
            {
                _machine.ChangeState(EnemyStateId.Chase);
                return;
            }

            if (distance < _context.Config.AttackRange)
                _machine.ChangeState(EnemyStateId.Attack);
        }

        public override void FixedTick(float fixedDeltaTime)
        {
            if (_context.PatrolPointCount == 0)
            {
                if (_context.HasPlayer)
                    _context.FaceTowards(_context.PlayerPosition);
                return;
            }

            if (_hasStoppedAtEnd)
            {
                _context.Steering.Stop();
                return;
            }

            Vector3 target = _context.GetPatrolPoint(_index);
            _context.Steering.Accelerate(target - _context.Position, _context.Config.MoveSpeed, _context.Config.Acceleration);
            _context.Audio.TickFootsteps(true);

            if (Vector3.Distance(_context.Position, target) < _context.Config.PatrolArriveRadius)
                AdvancePoint();
        }

        private void AdvancePoint()
        {
            // A one-point Loop would re-target the point it stands on and jitter there: it is a post, held like Once.
            if (_context.Config.PatrolMode == PatrolMode.Loop && _context.PatrolPointCount > 1)
            {
                _index = (_index + 1) % _context.PatrolPointCount;
                return;
            }

            if (_index < _context.PatrolPointCount - 1)
            {
                _index++;
                return;
            }

            _hasStoppedAtEnd = true;
            _context.Steering.Stop();
        }
    }
}
