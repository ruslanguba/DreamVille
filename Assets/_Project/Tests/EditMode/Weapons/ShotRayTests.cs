using System;
using NUnit.Framework;
using UnityEngine;

using DreamVille.Gameplay.Weapons;

namespace DreamVille.Tests.EditMode.Weapons
{
    public class ShotRayTests
    {
        [Test]
        public void Constructor_NormalizesDirection()
        {
            var ray = new ShotRay(new Vector3(1f, 2f, 3f), new Vector3(0f, 0f, 5f));

            Assert.That(ray.Origin, Is.EqualTo(new Vector3(1f, 2f, 3f)));
            Assert.That(ray.Direction, Is.EqualTo(Vector3.forward));
        }

        [Test]
        public void Constructor_ZeroDirection_Throws()
        {
            Assert.Throws<ArgumentException>(() => new ShotRay(Vector3.zero, Vector3.zero));
        }
    }
}
