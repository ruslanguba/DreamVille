using System;
using NUnit.Framework;

using DreamVille.Configs;
using DreamVille.Gameplay.Weapons.Pillow;
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
        public void PillowRack_InvalidConfig_Throws()
        {
            _config = PillowConfig.CreateForTests(arrivalThreshold: 0f);
            var bodies = new IPillowBody[] { new FakePillowBody() };

            Assert.Throws<ArgumentException>(() => new PillowRack(bodies, _config));
        }
    }
}
