using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay.Interactables;

namespace DreamVille.Tests.EditMode.Interactables
{
    /// <summary>The two factory methods of <see cref="InteractionContext"/> fill source and instigator.</summary>
    public class InteractionContextTests
    {
        private GameObject _instigator;

        [SetUp]
        public void SetUp()
        {
            _instigator = new GameObject("Instigator");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_instigator);
        }

        [Test]
        public void Shot_CarriesShotSourceAndInstigator()
        {
            InteractionContext context = InteractionContext.Shot(_instigator.transform);

            Assert.That(context.Source, Is.EqualTo(InteractionSource.Shot));
            Assert.That(context.Instigator, Is.SameAs(_instigator.transform));
        }

        [Test]
        public void Touch_CarriesTouchSourceAndInstigator()
        {
            InteractionContext context = InteractionContext.Touch(_instigator.transform);

            Assert.That(context.Source, Is.EqualTo(InteractionSource.Touch));
            Assert.That(context.Instigator, Is.SameAs(_instigator.transform));
        }
    }
}
