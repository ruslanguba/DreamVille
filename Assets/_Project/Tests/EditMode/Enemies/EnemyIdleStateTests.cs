using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Enemies.States;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary><see cref="EnemyIdleState"/>: an enemy that stops, idles and faces the player in one go.</summary>
    public class EnemyIdleStateTests
    {
        private EnemyConfig _config;
        private FakeEnemyContext _context;
        private EnemyStateMachine _machine;

        [SetUp]
        public void SetUp()
        {
            _config = EnemyConfig.CreateForTests();
            _context = new FakeEnemyContext { Config = _config };
            _machine = new EnemyStateMachine(machine => new IEnemyState[]
            {
                new EnemyIdleState(_context, machine),
                new EnemyFleeToHideoutState(_context, machine)
            });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
        }

        [Test]
        public void Enter_WithPlayer_SnapsFacing()
        {
            _context.HasPlayer = true;
            _context.PlayerPosition = new Vector3(4f, 0f, -3f);

            _machine.Start(EnemyStateId.Idle);

            Assert.That(_context.SnappedTo, Is.EqualTo(_context.PlayerPosition));
            Assert.That(_context.Steering.StopCount, Is.EqualTo(1));
            Assert.That(_context.AutoFacing, Is.False);
        }

        [Test]
        public void Exit_RestoresAutoFacing()
        {
            _machine.Start(EnemyStateId.Idle);

            _machine.ChangeState(EnemyStateId.FleeToHideout);

            Assert.That(_context.AutoFacing, Is.True);
        }
    }
}
