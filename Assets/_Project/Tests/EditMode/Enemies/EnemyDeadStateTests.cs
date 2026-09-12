using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Configs.Enemies;
using DreamVille.Gameplay.Enemies.States;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Enemies
{
    /// <summary><see cref="EnemyDeadState"/> against a real <see cref="EnemyStateMachine"/> and a <see cref="FakeEnemyContext"/>.</summary>
    public class EnemyDeadStateTests
    {
        private readonly List<Object> _createdObjects = new List<Object>();

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
        public void Enter_ReleasesBodyTriggersDeathAndSchedulesDespawn()
        {
            var context = new FakeEnemyContext { Config = Track(EnemyConfig.CreateForTests(despawnDelay: 4f)) };
            var machine = new EnemyStateMachine(m => new IEnemyState[] { new EnemyDeadState(context, m) });

            machine.Start(EnemyStateId.Dead);

            Assert.That(context.AutoFacing, Is.False);
            Assert.That(context.BodyReleased, Is.True);
            Assert.That(context.Animator.DeathTriggered, Is.True);
            Assert.That(context.DespawnDelay, Is.EqualTo(4f));
        }
    }
}
