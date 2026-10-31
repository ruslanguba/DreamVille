using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies;
using DreamVille.Gameplay.Enemies.States;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary><see cref="EnemyFleeToHideoutState"/> against a real <see cref="EnemyStateMachine"/> and a <see cref="FakeEnemyContext"/>.</summary>
    public class EnemyFleeToHideoutStateTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

        private FakeEnemyContext _context;
        private EnemyStateMachine _machine;

        [SetUp]
        public void SetUp()
        {
            _context = new FakeEnemyContext
            {
                Config = Track(EnemyConfig.CreateForTests(
                    hideoutArriveRadius: 2f, moveSpeed: 3f, chaseRange: 0f, attackRange: 0f, attackCooldown: 0f))
            };
            _context.Position = Vector3.zero;
            _context.HasFleeTarget = true;
            _context.FleeTargetPosition = new Vector3(1f, 0f, 0f);

            _machine = new EnemyStateMachine(machine => new IEnemyState[]
            {
                new EnemyFleeToHideoutState(_context, machine),
                new EnemyHiddenState(_context, machine),
                new EnemyIdleState(_context, machine)
            });
            _machine.Start(EnemyStateId.FleeToHideout);
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
        public void FixedTick_OnArrivalAtHideout_BecomesVulnerableAndTransitionsToHidden()
        {
            _context.FleeTargetIsHideout = true;

            _machine.FixedTick(0.1f);

            Assert.That(_context.Vulnerable, Is.True);
            Assert.That(_machine.CurrentState.Id, Is.EqualTo(EnemyStateId.Hidden));
            Assert.That(_context.HideoutEntered, Is.EqualTo(1));
        }

        [Test]
        public void FixedTick_OnArrivalAtWaypoint_TransitionsToIdle()
        {
            _context.FleeTargetIsHideout = false;

            _machine.FixedTick(0.1f);

            Assert.That(_machine.CurrentState.Id, Is.EqualTo(EnemyStateId.Idle));
        }
    }
}
