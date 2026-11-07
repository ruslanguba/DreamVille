using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Enemies.States;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary><see cref="EnemyPatrolState"/> against a real <see cref="EnemyStateMachine"/> and a <see cref="FakeEnemyContext"/>.</summary>
    public class EnemyPatrolStateTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        private FakeEnemyContext _context;
        private EnemyStateMachine _machine;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeEnemyContext { Config = Track(EnemyConfig.CreateForTests(patrolArriveRadius: 0.5f)) };

            _machine = new EnemyStateMachine(machine => new IEnemyState[]
            {
                new EnemyPatrolState(_context, machine),
                new EnemyChaseState(_context, machine),
                new EnemyAttackState(_context, machine)
            });
            _machine.Start(EnemyStateId.Patrol);
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
        public void Tick_WhenPlayerInsideChaseRange_TransitionsToChase()
        {
            _context.Config = Track(EnemyConfig.CreateForTests(moveSpeed: 3f, chaseRange: 10f, attackRange: 3f, attackCooldown: 2f));
            _context.HasPlayer = true;
            _context.Position = Vector3.zero;
            _context.PlayerPosition = new Vector3(5f, 0f, 0f);

            _machine.Tick(0.1f);

            Assert.That(_machine.CurrentState.Id, Is.EqualTo(EnemyStateId.Chase));
        }

        [Test]
        public void Tick_WhenPlayerInsideAttackRangeButOutsideChaseRange_TransitionsToAttack()
        {
            _context.Config = Track(EnemyConfig.CreateForTests(moveSpeed: 3f, chaseRange: 1f, attackRange: 5f, attackCooldown: 2f));
            _context.HasPlayer = true;
            _context.Position = Vector3.zero;
            _context.PlayerPosition = new Vector3(3f, 0f, 0f);

            _machine.Tick(0.1f);

            Assert.That(_machine.CurrentState.Id, Is.EqualTo(EnemyStateId.Attack));
        }

        [Test]
        public void Tick_WithoutPlayer_StaysInPatrol()
        {
            _context.Config = Track(EnemyConfig.CreateForTests(moveSpeed: 3f, chaseRange: 10f, attackRange: 5f, attackCooldown: 2f));
            _context.HasPlayer = false;

            _machine.Tick(0.1f);

            Assert.That(_machine.CurrentState.Id, Is.EqualTo(EnemyStateId.Patrol));
        }

        [Test]
        public void Tick_WhenChaseRangeIsZero_AttacksEvenAtPointBlank()
        {
            _context.Config = Track(EnemyConfig.CreateForTests(moveSpeed: 3f, chaseRange: 0f, attackRange: 5f, attackCooldown: 2f));
            _context.HasPlayer = true;
            _context.Position = Vector3.zero;
            _context.PlayerPosition = Vector3.zero;

            _machine.Tick(0.1f);

            Assert.That(_machine.CurrentState.Id, Is.EqualTo(EnemyStateId.Attack), "a zero chase range means 'never chases': Patrol must go straight to Attack");
        }

        [Test]
        public void FixedTick_WithEmptyRoute_FacesPlayerAndDoesNotSteer()
        {
            _context.HasPlayer = true;
            _context.PlayerPosition = new Vector3(1f, 0f, 2f);

            _machine.FixedTick(0.1f);

            Assert.That(_context.FacedTowards, Is.EqualTo(new[] { new Vector3(1f, 0f, 2f) }));
            Assert.That(_context.Steering.AccelerateCount, Is.EqualTo(0));
            Assert.That(_context.Steering.StopCount, Is.EqualTo(0));
        }

        [Test]
        public void FixedTick_WithRoute_SteersAtConfigSpeedAndAcceleration()
        {
            _context.PatrolPoints.Add(new Vector3(10f, 0f, 0f));
            _context.Config = Track(EnemyConfig.CreateForTests(acceleration: 7.5f, moveSpeed: 2.5f));

            _machine.FixedTick(0.1f);

            Assert.That(_context.Steering.LastTargetSpeed, Is.EqualTo(2.5f));
            Assert.That(_context.Steering.LastAcceleration, Is.EqualTo(7.5f));
        }

        [Test]
        public void FixedTick_OnArrival_AdvancesToNextPoint()
        {
            var pointA = new Vector3(0f, 0f, 0f);
            var pointB = new Vector3(5f, 0f, 0f);
            _context.PatrolPoints.Add(pointA);
            _context.PatrolPoints.Add(pointB);
            _context.Position = pointA;
            _context.Config = Track(EnemyConfig.CreateForTests(
                patrolArriveRadius: 0.5f, moveSpeed: 2f, chaseRange: 0f, attackRange: 0f, attackCooldown: 0f));

            _machine.FixedTick(0.1f);
            _machine.FixedTick(0.1f);

            Assert.That(_context.Steering.LastDirection, Is.EqualTo(pointB - pointA));
        }

        [Test]
        public void FixedTick_InLoopMode_WrapsAfterLastPoint()
        {
            var pointA = new Vector3(0f, 0f, 0f);
            var pointB = new Vector3(5f, 0f, 0f);
            _context.PatrolPoints.Add(pointA);
            _context.PatrolPoints.Add(pointB);
            _context.Config = Track(EnemyConfig.CreateForTests(
                patrolArriveRadius: 0.5f, moveSpeed: 2f, chaseRange: 0f, attackRange: 0f, attackCooldown: 0f));

            _context.Position = pointA;
            _machine.FixedTick(0.1f);
            _context.Position = pointB;
            _machine.FixedTick(0.1f);
            _context.Position = new Vector3(-10f, 0f, 0f);
            _machine.FixedTick(0.1f);

            Assert.That(_context.Steering.LastDirection, Is.EqualTo(pointA - _context.Position));
        }

        [Test]
        public void FixedTick_InOnceMode_StopsSteeringAtLastPoint()
        {
            _context.Config = Track(EnemyConfig.CreateForTests(
                patrolMode: PatrolMode.Once, patrolArriveRadius: 0.5f, moveSpeed: 2f, chaseRange: 0f, attackRange: 0f, attackCooldown: 0f));
            var pointA = new Vector3(0f, 0f, 0f);
            var pointB = new Vector3(5f, 0f, 0f);
            _context.PatrolPoints.Add(pointA);
            _context.PatrolPoints.Add(pointB);

            _context.Position = pointA;
            _machine.FixedTick(0.1f);
            _context.Position = pointB;
            _machine.FixedTick(0.1f);
            int accelerateCountAtArrival = _context.Steering.AccelerateCount;

            _machine.FixedTick(0.1f);

            Assert.That(_context.Steering.AccelerateCount, Is.EqualTo(accelerateCountAtArrival));
            Assert.That(_context.Steering.StopCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void Enter_AfterAChase_ResumesTheRoute()
        {
            var pointA = new Vector3(0f, 0f, 0f);
            var pointB = new Vector3(5f, 0f, 0f);
            _context.PatrolPoints.Add(pointA);
            _context.PatrolPoints.Add(pointB);
            _context.Config = Track(EnemyConfig.CreateForTests(
                patrolArriveRadius: 0.5f, moveSpeed: 2f, chaseRange: 7f, attackRange: 5f, attackCooldown: 2f));
            _context.Position = pointA;
            _machine.FixedTick(0.1f);
            _context.Position = new Vector3(2f, 0f, 0f);

            _machine.ChangeState(EnemyStateId.Chase);
            _machine.ChangeState(EnemyStateId.Patrol);
            _machine.FixedTick(0.1f);

            Assert.That(_context.Steering.LastDirection, Is.EqualTo(pointB - _context.Position));
        }

        [Test]
        public void FixedTick_OnAOnePointRoute_StopsAtThePoint()
        {
            var post = new Vector3(1f, 0f, 1f);
            _context.PatrolPoints.Add(post);
            _context.Config = Track(EnemyConfig.CreateForTests(
                patrolArriveRadius: 0.5f, moveSpeed: 2f, chaseRange: 0f, attackRange: 0f, attackCooldown: 0f));
            _context.Position = post;
            _machine.FixedTick(0.1f);
            int accelerateCountAtArrival = _context.Steering.AccelerateCount;

            _machine.FixedTick(0.1f);

            Assert.That(_context.Steering.AccelerateCount, Is.EqualTo(accelerateCountAtArrival));
            Assert.That(_context.Steering.StopCount, Is.GreaterThanOrEqualTo(1));
        }
    }
}
