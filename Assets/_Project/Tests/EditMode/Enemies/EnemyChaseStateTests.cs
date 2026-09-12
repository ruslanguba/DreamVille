using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Enemies.States;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary><see cref="EnemyChaseState"/> against a real <see cref="EnemyStateMachine"/> and a <see cref="FakeEnemyContext"/>.</summary>
    public class EnemyChaseStateTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        private FakeEnemyContext _context;
        private EnemyStateMachine _machine;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeEnemyContext { Config = Track(EnemyConfig.CreateForTests()) };

            _machine = new EnemyStateMachine(machine => new IEnemyState[]
            {
                new EnemyPatrolState(_context, machine),
                new EnemyChaseState(_context, machine),
                new EnemyAttackState(_context, machine)
            });
            _machine.Start(EnemyStateId.Chase);

            _context.HasPlayer = true;
            _context.Position = Vector3.zero;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (Object created in _createdObjects)
                Object.DestroyImmediate(created);

            _createdObjects.Clear();
        }

        private T Track<T>(T asset) where T : Object
        {
            _createdObjects.Add(asset);
            return asset;
        }

        [Test]
        public void Tick_WhenPlayerLeavesChaseRange_ReturnsToPatrol()
        {
            _context.Tuning = new EnemyTuning(moveSpeed: 3f, chaseRange: 5f, attackRange: 2f, attackCooldown: 2f);
            _context.PlayerPosition = new Vector3(10f, 0f, 0f);

            _machine.Tick(0.1f);

            Assert.That(_machine.CurrentState.Id, Is.EqualTo(EnemyStateId.Patrol));
        }

        [Test]
        public void Tick_WhenPlayerEntersAttackRange_TransitionsToAttack()
        {
            _context.Tuning = new EnemyTuning(moveSpeed: 3f, chaseRange: 5f, attackRange: 3f, attackCooldown: 2f);
            _context.PlayerPosition = new Vector3(1f, 0f, 0f);

            _machine.Tick(0.1f);

            Assert.That(_machine.CurrentState.Id, Is.EqualTo(EnemyStateId.Attack));
        }

        [Test]
        public void FixedTick_SteersTowardsPlayerAtTunedSpeedAndConfigAcceleration()
        {
            _context.Tuning = new EnemyTuning(moveSpeed: 4f, chaseRange: 5f, attackRange: 1f, attackCooldown: 2f);
            _context.PlayerPosition = new Vector3(2f, 0f, 3f);

            _machine.FixedTick(0.1f);

            Assert.That(_context.Steering.LastDirection, Is.EqualTo(new Vector3(2f, 0f, 3f)));
            Assert.That(_context.Steering.LastTargetSpeed, Is.EqualTo(4f));
            Assert.That(_context.Steering.LastAcceleration, Is.EqualTo(_context.Config.Acceleration));
            Assert.That(_context.Audio.FootstepTickCount, Is.EqualTo(0));
        }
    }
}
