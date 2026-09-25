using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using DreamVille.Gameplay.Interactables;
using DreamVille.Tests.EditMode.Fakes;

using Object = UnityEngine.Object;

namespace DreamVille.Tests.EditMode.Interactables
{
    /// <summary>Order, context forwarding and fault isolation of <see cref="EffectChain"/>.</summary>
    public class EffectChainTests
    {
        private GameObject _bullet;
        private List<int> _log;

        [SetUp]
        public void SetUp()
        {
            _bullet = new GameObject("Bullet");
            _log = new List<int>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_bullet);
        }

        [Test]
        public void Apply_RunsEveryEffectInOrder()
        {
            var chain = new EffectChain(new IInteractionEffect[]
            {
                new RecordingEffect(1, _log),
                new RecordingEffect(2, _log),
                new RecordingEffect(3, _log)
            });

            chain.Apply(InteractionContext.Shot(_bullet.transform));

            Assert.That(_log, Is.EqualTo(new[] { 1, 2, 3 }));
        }

        [Test]
        public void Apply_PassesTheSameContextToEveryEffect()
        {
            var first = new RecordingEffect(1, _log);
            var second = new RecordingEffect(2, _log);
            var chain = new EffectChain(new IInteractionEffect[] { first, second });

            chain.Apply(InteractionContext.Touch(_bullet.transform));

            foreach (RecordingEffect effect in new[] { first, second })
            {
                Assert.That(effect.LastContext.Source, Is.EqualTo(InteractionSource.Touch));
                Assert.That(effect.LastContext.Instigator, Is.SameAs(_bullet.transform));
            }
        }

        [Test]
        public void Apply_WhenAnEffectThrows_LogsAndStillRunsTheRest()
        {
            var after = new RecordingEffect(2, _log);
            var chain = new EffectChain(new IInteractionEffect[] { new RecordingEffect(1, _log), new ThrowingEffect(), after });
            LogAssert.Expect(LogType.Exception, new Regex(Regex.Escape(ThrowingEffect.Message)));

            chain.Apply(InteractionContext.Shot(_bullet.transform));

            Assert.That(after.CallCount, Is.EqualTo(1));
            Assert.That(_log, Is.EqualTo(new[] { 1, 2 }));
        }

        [Test]
        public void Constructor_WithNullList_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new EffectChain(null));
        }

        [Test]
        public void Constructor_CopiesTheList()
        {
            var source = new List<IInteractionEffect> { new RecordingEffect(1, _log) };
            var chain = new EffectChain(source);

            source.Add(new RecordingEffect(2, _log));

            Assert.That(chain.Count, Is.EqualTo(1));
        }
    }
}
