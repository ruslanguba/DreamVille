using System;
using NUnit.Framework;

using DreamVille.Configs;
using DreamVille.Gameplay.Weapons.Pillows;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Weapons
{
    public class PillowConfigTests
    {
        private PillowConfig _config;

        [TearDown]
        public void TearDown()
        {
            if (_config != null)
                UnityEngine.Object.DestroyImmediate(_config);
        }

        [Test]
        public void IsValid_Defaults_True()
        {
            _config = PillowConfig.CreateForTests();

            bool valid = _config.IsValid(out string error);

            Assert.That(valid, Is.True);
            Assert.That(error, Is.Null);
        }

        [Test]
        public void IsValid_NonPositiveRecallSpeed_FalseWithReason()
        {
            _config = PillowConfig.CreateForTests(recallSpeed: 0f);

            bool valid = _config.IsValid(out string error);

            Assert.That(valid, Is.False);
            Assert.That(error, Does.Contain(nameof(PillowConfig.RecallSpeed)));
        }

        [Test]
        public void IsValid_NaNRecallSpeed_False()
        {
            _config = PillowConfig.CreateForTests(recallSpeed: float.NaN);

            bool valid = _config.IsValid(out string error);

            Assert.That(valid, Is.False);
            Assert.That(error, Does.Contain(nameof(PillowConfig.RecallSpeed)));
        }

        [Test]
        public void IsValid_NegativeSnapDistance_False()
        {
            _config = PillowConfig.CreateForTests(recallSnapDistance: -1f);

            bool valid = _config.IsValid(out string error);

            Assert.That(valid, Is.False);
            Assert.That(error, Does.Contain(nameof(PillowConfig.RecallSnapDistance)));
        }

        [Test]
        public void IsValid_NegativeThrowImpulse_False()
        {
            _config = PillowConfig.CreateForTests(throwImpulse: -1f);

            bool valid = _config.IsValid(out string error);

            Assert.That(valid, Is.False);
            Assert.That(error, Does.Contain(nameof(PillowConfig.ThrowImpulse)));
        }

        [Test]
        public void PillowRack_InvalidConfig_Throws()
        {
            _config = PillowConfig.CreateForTests(arrivalThreshold: 0f);
            var bodies = new IPillowBody[] { new FakePillowBody() };

            Assert.Throws<ArgumentException>(() => new PillowRack(bodies, _config));
        }
    }
}
