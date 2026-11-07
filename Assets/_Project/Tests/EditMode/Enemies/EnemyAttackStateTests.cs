using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Enemies.States;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary><see cref="EnemyAttackState"/> against a real <see cref="EnemyStateMachine"/> and a <see cref="FakeEnemyContext"/>.</summary>
    public class EnemyAttackStateTests
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
        public void Enter_StopsSnapsFacingAndDisablesAutoFacing()
        {
            _context.HasPlayer = true;
            _context.PlayerPosition = new Vector3(3f, 0f, 4f);

            _machine.Start(EnemyStateId.Attack);

            Assert.That(_context.Steering.StopCount, Is.EqualTo(1));
            Assert.That(_context.AutoFacing, Is.False);
            Assert.That(_context.SnappedTo, Is.EqualTo(_context.PlayerPosition));
        }

        [Test]
        public void Tick_ThrowsOncePerCooldown()
        {
            _context.Config = Track(EnemyConfig.CreateForTests(moveSpeed: 0f, chaseRange: 0f, attackRange: 100f, attackCooldown: 2f));
            _context.HasPlayer = true;
            _context.Attack.Origin = Vector3.zero;
            _context.PlayerPosition = new Vector3(5f, 0f, 0f);
            _machine.Start(EnemyStateId.Attack);

            _machine.Tick(0.5f);

            Assert.That(_context.Attack.ThrownDirections.Count, Is.EqualTo(1));
            Assert.That(_context.Audio.AttackCount, Is.EqualTo(1));

            _machine.Tick(0.5f);
            _machine.Tick(0.5f);
            _machine.Tick(0.5f);

            Assert.That(_context.Attack.ThrownDirections.Count, Is.EqualTo(1));

            // 0.5 s of cooldown is left; leaving and re-entering the state must not reset it.
            _machine.ChangeState(EnemyStateId.Chase);
            _machine.ChangeState(EnemyStateId.Attack);
            _machine.Tick(0.1f);

            Assert.That(_context.Attack.ThrownDirections.Count, Is.EqualTo(1), "re-entering Attack reset the cooldown");

            _machine.Tick(0.5f);

            Assert.That(_context.Attack.ThrownDirections.Count, Is.EqualTo(2));
            Assert.That(_context.Audio.AttackCount, Is.EqualTo(2));
        }

        [Test]
        public void Tick_WhenPlayerLeavesAttackRangeWithoutChaseRange_PatrolsAndRestoresAutoFacing()
        {
            _context.Config = Track(EnemyConfig.CreateForTests(moveSpeed: 0f, chaseRange: 0f, attackRange: 3f, attackCooldown: 2f));
            _context.HasPlayer = true;
            _context.PlayerPosition = new Vector3(10f, 0f, 0f);
            _machine.Start(EnemyStateId.Attack);

            _machine.Tick(0.1f);

            Assert.That(_machine.CurrentState.Id, Is.EqualTo(EnemyStateId.Patrol));
            Assert.That(_context.AutoFacing, Is.True);
        }

        [Test]
        public void Tick_WhenPlayerLeavesAttackRangeWithinChaseRange_Chases()
        {
            _context.Config = Track(EnemyConfig.CreateForTests(moveSpeed: 5f, chaseRange: 7f, attackRange: 5f, attackCooldown: 4f));
            _context.HasPlayer = true;
            _context.PlayerPosition = new Vector3(6f, 0f, 0f);
            _machine.Start(EnemyStateId.Attack);

            _machine.Tick(0.1f);

            Assert.That(_machine.CurrentState.Id, Is.EqualTo(EnemyStateId.Chase));
        }

        [Test]
        public void Tick_JustBeyondAttackRange_KeepsAttacking()
        {
            _context.Config = Track(EnemyConfig.CreateForTests(moveSpeed: 5f, chaseRange: 7f, attackRange: 5f, attackCooldown: 4f));
            _context.HasPlayer = true;
            _context.PlayerPosition = new Vector3(5.3f, 0f, 0f);
            _machine.Start(EnemyStateId.Attack);

            _machine.Tick(0.1f);

            Assert.That(_machine.CurrentState.Id, Is.EqualTo(EnemyStateId.Attack));
        }
    }
}
